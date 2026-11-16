using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.RecordForms.CrossPageRules;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Rules;

namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// Loads a whole revision and runs every check on it, so check your answers shows the same errors
/// Submit would return. Loading and checking are separate, so Submit can reject a stale hash
/// before paying for the checks.
/// </summary>
internal sealed class RevisionChecker(
    RecordFormPageLoader loader,
    CrossPageRuleRegistry crossPageRules,
    IDateTimeProvider dateTimeProvider
)
{
    /// <summary>Loads every answer on the revision, works out what applies, and hashes it.</summary>
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
