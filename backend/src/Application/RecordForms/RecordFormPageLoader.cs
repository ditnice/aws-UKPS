using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// The steps the page query and save services share: finding and authorising the page, and
/// reading its rows, answers and options.
/// </summary>
internal sealed class RecordFormPageLoader(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser,
    FormModelRegistry formModels,
    OptionProviderRegistry optionProviders
)
{
    /// <summary>
    /// Finds the record's latest revision and the page, and checks the caller can read them. A
    /// record has at most one draft revision, which is always its latest. The revision is tracked
    /// when <paramref name="track"/> is set, for a save.
    /// </summary>
    public async Task<(LoadedPage? Page, PageLookupFailure Failure)> LoadPageAsync(
        int recordId,
        string pageId,
        bool track,
        CancellationToken cancellationToken
    )
    {
        IQueryable<RecordRevision> revisions = track
            ? dbContext.RecordRevisions
            : dbContext.RecordRevisions.AsNoTracking();
        RecordRevision? revision = await revisions
            .Include(static r => r.Record)
            .Where(r => r.RecordId == recordId)
            .OrderByDescending(static r => r.CreatedAt)
            .ThenByDescending(static r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (revision?.Record is not { } record)
        {
            return (null, PageLookupFailure.NotFound);
        }

        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Read,
                record.OrganisationId
            )
        )
        {
            return (null, PageLookupFailure.NotAllowed);
        }

        FormModel? model = formModels.Find(record.RecordType);
        FormPage? page = model?.FindPage(pageId);
        if (model is null || page is null)
        {
            return (null, PageLookupFailure.NotFound);
        }

        bool canEdit = organisationAuthoriser.CanPerformOperationOnOrganisation(
            Operation.EditRecordContent,
            record.OrganisationId
        );
        return (new LoadedPage(revision, record, model, page, canEdit), PageLookupFailure.NotFound);
    }

    /// <summary>
    /// Loads the revision's row of every entity on the page, tracked when
    /// <paramref name="track"/> is set, for a save.
    /// </summary>
    public async Task<Dictionary<FormEntity, object?>> LoadRowsAsync(
        FormPage page,
        int revisionId,
        bool track,
        CancellationToken cancellationToken
    )
    {
        Dictionary<FormEntity, object?> rows = [];
        foreach (FormEntity entity in page.Entities)
        {
            rows[entity] = await entity.LoadAsync(dbContext, revisionId, track, cancellationToken);
        }

        return rows;
    }

    public static Dictionary<string, FormAnswer> ReadAnswers(
        FormPage page,
        Dictionary<FormEntity, object?> rows
    ) =>
        page.Questions.ToDictionary(
            static q => q.Id,
            q => q.Binding.Read(rows[q.Entity]),
            StringComparer.Ordinal
        );

    /// <summary>
    /// Loads the options of every choice question. Saved answers are passed in so archived items
    /// already chosen stay offered.
    /// </summary>
    public async Task<Dictionary<string, IReadOnlyList<FormOption>>> LoadOptionsAsync(
        FormPage page,
        Dictionary<string, FormAnswer> savedAnswers,
        CancellationToken cancellationToken
    )
    {
        Dictionary<string, IReadOnlyList<FormOption>> options = new(StringComparer.Ordinal);
        foreach (FormQuestion question in page.Questions.Where(static q => q.HasOptions))
        {
            IOptionProvider provider =
                optionProviders.Find(question.OptionKey!)
                ?? throw new InvalidOperationException(
                    $"Option provider '{question.OptionKey}' is not registered."
                );
            options[question.Id] = await provider.GetOptionsAsync(
                dbContext,
                savedAnswers[question.Id].AllValues().ToArray(),
                cancellationToken
            );
        }

        return options;
    }
}
