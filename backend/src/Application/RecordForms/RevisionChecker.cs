using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.RecordForms.CrossPageRules;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Rules;

namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// Decides whether a revision can be submitted at all, loads it, and runs every check on it, so
/// check your answers refuses what Submit refuses and shows the same errors Submit would return.
/// Loading and checking are separate, so Submit can reject a stale hash before paying for the
/// checks.
/// </summary>
internal sealed class RevisionChecker(
    RecordFormPageLoader loader,
    CrossPageRuleRegistry crossPageRules,
    IOrganisationAuthoriser organisationAuthoriser,
    IDateTimeProvider dateTimeProvider
)
{
    /// <summary>
    /// Why the caller can't submit the form's latest revision whatever its answers, or null when
    /// nothing stops it.
    /// </summary>
    public SubmitBlocker? FindSubmitBlocker(LoadedForm form)
    {
        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.EditRecordContent,
                form.Record.OrganisationId
            )
        )
        {
            return SubmitBlocker.CannotSubmit;
        }

        return form.Record.CanSubmit(form.Revision) ? null : SubmitBlocker.NotSubmittable;
    }

    /// <summary>Loads every answer on the revision, works out what applies, and hashes it.</summary>
    /// <remarks>
    /// The tables are read one after another, not from one snapshot, so a save that lands part
    /// way through can give a hash of a state that never existed. That is safe only because
    /// Submit loads and hashes the revision again and rejects any mismatch; don't skip that.
    /// </remarks>
    public async Task<LoadedRevision> LoadAsync(
        LoadedForm form,
        CancellationToken cancellationToken
    )
    {
        var (answers, applicability) = await loader.LoadAllAnswersAsync(form, cancellationToken);
        return new LoadedRevision(
            answers,
            applicability,
            AnswerHash.ForRevision(form.Revision.Id, answers)
        );
    }

    /// <summary>
    /// Runs every page rule on the pages that apply, then every cross-page rule, loading the
    /// options of the choice questions they check.
    /// </summary>
    public async Task<RevisionCheck> CheckAsync(
        LoadedForm form,
        LoadedRevision revision,
        CancellationToken cancellationToken
    )
    {
        var options = await LoadOptionsAsync(form.Model, revision, cancellationToken);
        return new RevisionCheck(
            options,
            RevisionValidator.Validate(
                form.Model,
                revision.Answers,
                revision.Applicability,
                options,
                crossPageRules.For(form.Record.RecordType),
                DateAnswer.Today(dateTimeProvider.GetOffsetUtcNow())
            )
        );
    }

    /// <summary>The options of every choice question on the pages that apply.</summary>
    private async Task<Dictionary<string, IReadOnlyList<FormOption>>> LoadOptionsAsync(
        FormModel model,
        LoadedRevision revision,
        CancellationToken cancellationToken
    )
    {
        Dictionary<string, IReadOnlyList<FormOption>> options = new(StringComparer.Ordinal);
        foreach (FormPage page in model.Pages.Where(revision.Applicability.Applies))
        {
            var pageOptions = await loader.LoadOptionsAsync(
                page,
                PageAnswers.For(page, revision.Answers),
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
