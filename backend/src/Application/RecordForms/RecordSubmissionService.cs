using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.CrossPageRules;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Rules;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Application.RecordForms;

internal sealed class RecordSubmissionService(
    AppDbContext dbContext,
    RecordFormPageLoader loader,
    CrossPageRuleRegistry crossPageRules,
    IOrganisationAuthoriser organisationAuthoriser,
    CurrentDbUserEntityService currentDbUserEntityService,
    IDateTimeProvider dateTimeProvider
) : IRecordSubmissionService
{
    public async Task<SubmitRecordResult> Submit(
        int recordId,
        SubmitRecordCommand command,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        var (form, failure) = await loader.LoadFormAsync(recordId, track: true, cancellationToken);
        if (form is null)
        {
            return SubmitRecordResult.Err(
                failure == PageLookupFailure.NotAllowed
                    ? new SubmitRecordError.NotAllowed()
                    : new SubmitRecordError.NotFound()
            );
        }

        if (CheckCanSubmit(form, command) is { } error)
        {
            return SubmitRecordResult.Err(error);
        }

        // The hash is checked before the rules, so a user who hasn't seen the latest answers
        // reloads straight away rather than fixing errors in answers that have since changed.
        var (answers, applicability) = await loader.LoadAllAnswersAsync(form, cancellationToken);
        string hash = AnswerHash.ForRevision(form.Revision.Id, answers);
        if (!string.Equals(command.RevisionHash, hash, StringComparison.Ordinal))
        {
            return SubmitRecordResult.Err(new SubmitRecordError.RecordChanged());
        }

        // Checked again even when the hash matches, as date rules change with the date.
        IReadOnlyList<SubmissionErrorDto> errors = RevisionValidator.Validate(
            form.Model,
            answers,
            applicability,
            await LoadOptionsAsync(form.Model, answers, applicability, cancellationToken),
            crossPageRules.For(form.Record.RecordType),
            DateAnswer.Today(dateTimeProvider.GetOffsetUtcNow())
        );
        if (errors.Count > 0)
        {
            return SubmitRecordResult.Err(new SubmitRecordError.Invalid(errors));
        }

        User user = await currentDbUserEntityService.GetCurrentUser(cancellationToken);
        if (!form.Record.TrySubmit(form.Revision, dateTimeProvider.GetUtcNow(), user))
        {
            return SubmitRecordResult.Err(new SubmitRecordError.NotSubmittable());
        }

        // The revision's status changes, so its row is updated against the version read with it.
        // Every page save touches that row too, so a save that landed since it was read fails the
        // submission, and a later save fails against the submitted revision.
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return SubmitRecordResult.Err(new SubmitRecordError.RecordChanged());
        }

        return SubmitRecordResult.Ok();
    }

    private SubmitRecordError? CheckCanSubmit(LoadedForm form, SubmitRecordCommand command)
    {
        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.EditRecordContent,
                form.Record.OrganisationId
            )
        )
        {
            return new SubmitRecordError.CannotSubmit();
        }

        if (!form.Record.CanSubmit(form.Revision))
        {
            return new SubmitRecordError.NotSubmittable();
        }

        if (!string.Equals(command.FormVersion, form.Model.Version, StringComparison.Ordinal))
        {
            return new SubmitRecordError.FormVersionChanged();
        }

        return null;
    }

    /// <summary>The options of every choice question on the pages that apply.</summary>
    private async Task<Dictionary<string, IReadOnlyList<FormOption>>> LoadOptionsAsync(
        FormModel model,
        IReadOnlyDictionary<string, FormAnswer> answers,
        Applicability applicability,
        CancellationToken cancellationToken
    )
    {
        Dictionary<string, IReadOnlyList<FormOption>> options = new(StringComparer.Ordinal);
        foreach (FormPage page in model.Pages.Where(applicability.Applies))
        {
            var pageOptions = await loader.LoadOptionsAsync(
                page,
                PageAnswers.For(page, answers),
                cancellationToken
            );
            foreach (var (questionId, questionOptions) in pageOptions)
            {
                options[questionId] = questionOptions;
            }
        }

        return options;
    }
}
