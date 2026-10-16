using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Forms.Bindings;
using UKPS.Api.Application.Forms.Definitions;
using UKPS.Api.Application.Forms.Dtos;
using UKPS.Api.Application.Forms.Errors;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using SaveRecordPageResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Forms.Dtos.SaveRecordPageDto,
    UKPS.Api.Application.Forms.Errors.SaveRecordPageError
>;

namespace UKPS.Api.Application.Forms;

/// <summary>
/// Saves a whole form page: authorise, check versions, validate every answer, write what
/// changed, audit it, and return the next page. Everything is saved in one
/// <c>SaveChangesAsync</c>, so a failure leaves nothing written.
/// </summary>
internal sealed class RecordPageSaveService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser,
    FormDefinitionRegistry formDefinitions,
    CurrentDbUserEntityService currentDbUserEntityService,
    IDateTimeProvider dateTimeProvider
) : IRecordPageSaveService
{
    public async Task<SaveRecordPageResult> SavePage(
        int recordId,
        int revisionId,
        string pageId,
        SaveRecordPageCommand command,
        CancellationToken cancellationToken
    )
    {
        var revision = await dbContext
            .RecordRevisions.Include(x => x.Record)
            .SingleOrDefaultAsync(
                x => x.Id == revisionId && x.RecordId == recordId,
                cancellationToken
            );

        var (error, form, page) = CheckCanSave(revision, pageId, command);
        if (error is not null)
        {
            return SaveRecordPageResult.Err(error);
        }

        var bindingContext = new BindingContext(dbContext, revisionId);
        var before = Normalise(
            page!,
            await AnswerReader.ReadAsync(bindingContext, page!.Questions, cancellationToken)
        );

        var errors = PageAnswerValidator.CheckShape(page, command.Answers);
        if (errors.Count > 0)
        {
            return SaveRecordPageResult.Err(new SaveRecordPageError.Invalid(errors));
        }

        var after = Normalise(page, command.Answers);
        await CheckOptionsAsync(page, before, after, errors, cancellationToken);
        PageAnswerValidator.ValidateRules(page, after, errors);
        if (errors.Count > 0)
        {
            return SaveRecordPageResult.Err(new SaveRecordPageError.Invalid(errors));
        }

        var changed = page
            .Questions.Where(q => !AnswerValues.AreEqual(before[q.Id], after[q.Id]))
            .ToList();
        if (
            changed.Count > 0
            && await WriteChangesAsync(
                bindingContext,
                revision!,
                page,
                changed,
                before,
                after,
                cancellationToken
            )
                is { } saveError
        )
        {
            return SaveRecordPageResult.Err(saveError);
        }

        return SaveRecordPageResult.Ok(
            new SaveRecordPageDto { NextPageId = form!.NextPage(page)?.Id }
        );
    }

    private async Task<SaveRecordPageError?> WriteChangesAsync(
        BindingContext bindingContext,
        RecordRevision revision,
        PageDefinition page,
        List<QuestionDefinition> changed,
        Dictionary<string, JsonNode?> before,
        Dictionary<string, JsonNode?> after,
        CancellationToken cancellationToken
    )
    {
        foreach (var question in changed)
        {
            await question.Binding.WriteAsync(
                bindingContext,
                after[question.Id],
                cancellationToken
            );
        }

        await StampAndAuditAsync(revision, page, changed, before, after, cancellationToken);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return null;
        }
        catch (DbUpdateConcurrencyException)
        {
            return new SaveRecordPageError.RevisionChanged();
        }
    }

    private (SaveRecordPageError? Error, FormDefinition? Form, PageDefinition? Page) CheckCanSave(
        RecordRevision? revision,
        string pageId,
        SaveRecordPageCommand command
    )
    {
        if (revision?.Record is not { } record)
        {
            return (new SaveRecordPageError.RecordNotFound(), null, null);
        }

        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Read,
                record.OrganisationId
            )
        )
        {
            return (new SaveRecordPageError.NotAllowed(), null, null);
        }

        var form = formDefinitions.Get(record.RecordType);
        var page = form?.FindPage(pageId);
        if (form is null || page is null)
        {
            return (new SaveRecordPageError.PageNotFound(), null, null);
        }

        return (CheckCanEdit(revision, record.OrganisationId, form, command), form, page);
    }

    private SaveRecordPageError? CheckCanEdit(
        RecordRevision revision,
        int organisationId,
        FormDefinition form,
        SaveRecordPageCommand command
    )
    {
        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.EditRecordContent,
                organisationId
            )
        )
        {
            return new SaveRecordPageError.EditNotAllowed();
        }

        if (revision.WorkflowStatus != WorkflowStatus.Draft)
        {
            return new SaveRecordPageError.RevisionNotDraft();
        }

        if (!string.Equals(command.FormVersion, form.Version, StringComparison.Ordinal))
        {
            return new SaveRecordPageError.FormVersionChanged();
        }

        return command.RevisionVersion == revision.Version
            ? null
            : new SaveRecordPageError.RevisionChanged();
    }

    private static Dictionary<string, JsonNode?> Normalise(
        PageDefinition page,
        IReadOnlyDictionary<string, JsonNode?> answers
    ) =>
        page.Questions.ToDictionary(
            q => q.Id,
            q => q.Type.Normalise(answers[q.Id]),
            StringComparer.Ordinal
        );

    /// <summary>
    /// Every selected value must be a currently offered option. Archived reference data counts
    /// as offered only if it was already saved.
    /// </summary>
    private async Task CheckOptionsAsync(
        PageDefinition page,
        Dictionary<string, JsonNode?> before,
        Dictionary<string, JsonNode?> after,
        Dictionary<string, string[]> errors,
        CancellationToken cancellationToken
    )
    {
        foreach (var question in page.Questions.Where(q => q.Options is not null))
        {
            var offered = await question.Options!.LoadAsync(
                dbContext,
                AnswerReader.SelectedValues(before[question.Id]),
                cancellationToken
            );
            var offeredValues = offered.Select(o => o.Value).ToHashSet(StringComparer.Ordinal);
            if (!AnswerReader.SelectedValues(after[question.Id]).All(offeredValues.Contains))
            {
                errors[question.Id] = [PageAnswerValidator.InvalidOption];
            }
        }
    }

    private async Task StampAndAuditAsync(
        RecordRevision revision,
        PageDefinition page,
        IEnumerable<QuestionDefinition> changed,
        Dictionary<string, JsonNode?> before,
        Dictionary<string, JsonNode?> after,
        CancellationToken cancellationToken
    )
    {
        var user = await currentDbUserEntityService.GetCurrentUser(cancellationToken);
        var now = dateTimeProvider.GetUtcNow();

        // Updating the revision also bumps its concurrency token for the next save.
        revision.UpdatedAt = now;
        revision.UpdatedBy = user.Id;

        dbContext.RecordEvents.Add(
            new RecordEvent
            {
                RecordId = revision.RecordId,
                RevisionId = revision.Id,
                EventType = RecordEventType.RecordContentUpdated,
                PerformedBy = user.Id,
                PerformedAt = now,
                Note = page.Id,
                FieldChanges =
                [
                    .. changed.Select(question => new RecordEventFieldChange
                    {
                        FieldPath = question.Id,
                        OldValue = AnswerValues.ToAuditValue(before[question.Id]),
                        NewValue = AnswerValues.ToAuditValue(after[question.Id]),
                    }),
                ],
            }
        );
    }
}
