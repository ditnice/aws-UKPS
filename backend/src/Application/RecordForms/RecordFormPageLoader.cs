using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// The steps the form services share: finding and authorising the record's form or one of its
/// pages, and reading a page's rows, answers and options.
/// </summary>
internal sealed class RecordFormPageLoader(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser,
    FormModelRegistry formModels,
    OptionProviderRegistry optionProviders
)
{
    private static readonly MethodInfo _loadCodesMethod = typeof(RecordFormPageLoader).GetMethod(
        nameof(LoadCodesAsync),
        BindingFlags.NonPublic | BindingFlags.Static
    )!;

    /// <summary>
    /// Finds the record's latest revision and the page, and checks the caller can read them. The
    /// revision is tracked when <paramref name="track"/> is set, for a save.
    /// </summary>
    public async Task<(LoadedPage? Page, PageLookupFailure Failure)> LoadPageAsync(
        int recordId,
        string pageId,
        bool track,
        CancellationToken cancellationToken
    )
    {
        var (form, failure) = await LoadFormAsync(recordId, track, cancellationToken);
        if (form is null)
        {
            return (null, failure);
        }

        FormPage? page = form.Model.FindPage(pageId);
        if (page is null)
        {
            return (null, PageLookupFailure.NotFound);
        }

        bool canEdit = organisationAuthoriser.CanPerformOperationOnOrganisation(
            Operation.EditRecordContent,
            form.Record.OrganisationId
        );
        return (
            new LoadedPage(form.Revision, form.Record, form.Model, page, canEdit),
            PageLookupFailure.NotFound
        );
    }

    /// <summary>
    /// Finds the record's latest revision and its form, and checks the caller can read them. A
    /// record has at most one draft revision, which is always its latest. The revision is tracked
    /// when <paramref name="track"/> is set, for a save.
    /// </summary>
    public async Task<(LoadedForm? Form, PageLookupFailure Failure)> LoadFormAsync(
        int recordId,
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
        return model is null
            ? (null, PageLookupFailure.NotFound)
            : (new LoadedForm(revision, record, model), PageLookupFailure.NotFound);
    }

    /// <summary>
    /// Loads the rows and answers the page needs and works out what applies, or returns null when
    /// the page does not apply, which callers treat as not found. A save loads, and tracks, the
    /// rows of every answer it may clear as well.
    /// </summary>
    public async Task<LoadedAnswers?> LoadAnswersAsync(
        LoadedPage loaded,
        bool forSave,
        CancellationToken cancellationToken
    )
    {
        FormModel model = loaded.Model;
        FormPage page = loaded.Page;
        IReadOnlyList<FormEntity> entities = forSave
            ? model.EntitiesToSave(page)
            : model.EntitiesToRead(page);

        var (rows, answers) = await LoadRowsAsync(
            model,
            entities,
            loaded.Revision.Id,
            track: forSave,
            cancellationToken
        );
        ReferenceCodes codes = await LoadReferenceCodesAsync(model, cancellationToken);
        Applicability applicability = FormApplicability.Evaluate(model, answers, codes);
        return applicability.Applies(page)
            ? new LoadedAnswers(rows, answers, applicability, codes)
            : null;
    }

    /// <summary>
    /// Works out which of the form's pages and questions apply to the revision, loading only the
    /// rows of <see cref="FormModel.TriggerEntities"/>.
    /// </summary>
    public async Task<Applicability> LoadApplicabilityAsync(
        FormModel model,
        int revisionId,
        CancellationToken cancellationToken
    )
    {
        var (_, answers) = await LoadRowsAsync(
            model,
            model.TriggerEntities,
            revisionId,
            track: false,
            cancellationToken
        );
        ReferenceCodes codes = await LoadReferenceCodesAsync(model, cancellationToken);
        return FormApplicability.Evaluate(model, answers, codes);
    }

    /// <summary>Loads the codes of the reference data that the form's conditions name.</summary>
    private async Task<ReferenceCodes> LoadReferenceCodesAsync(
        FormModel model,
        CancellationToken cancellationToken
    )
    {
        if (model.ReferenceTriggerTypes.Count == 0)
        {
            return ReferenceCodes.None;
        }

        Dictionary<Type, IReadOnlyList<(int Id, string Code)>> rows = [];
        foreach (Type type in model.ReferenceTriggerTypes)
        {
            rows[type] = await (Task<IReadOnlyList<(int Id, string Code)>>)
                _loadCodesMethod
                    .MakeGenericMethod(type)
                    .Invoke(null, [dbContext, cancellationToken])!;
        }

        return new ReferenceCodes(rows);
    }

    private static async Task<IReadOnlyList<(int Id, string Code)>> LoadCodesAsync<TEntity>(
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
        where TEntity : class
    {
        var rows = await dbContext
            .Set<TEntity>()
            .AsNoTracking()
            .Select(static x => new
            {
                Id = EF.Property<int>(x, "Id"),
                Code = EF.Property<string>(x, "Code"),
            })
            .ToListAsync(cancellationToken);
        return [.. rows.Select(static x => (x.Id, x.Code))];
    }

    /// <summary>
    /// Loads the revision's row of each entity, and the answer of every question stored in them.
    /// </summary>
    private async Task<(
        Dictionary<FormEntity, object?> Rows,
        Dictionary<string, FormAnswer> Answers
    )> LoadRowsAsync(
        FormModel model,
        IEnumerable<FormEntity> entities,
        int revisionId,
        bool track,
        CancellationToken cancellationToken
    )
    {
        Dictionary<FormEntity, object?> rows = [];
        foreach (FormEntity entity in entities)
        {
            rows[entity] = await entity.LoadAsync(dbContext, revisionId, track, cancellationToken);
        }

        Dictionary<string, FormAnswer> answers = model
            .Questions.Where(q => rows.ContainsKey(q.Entity))
            .ToDictionary(
                static q => q.Id,
                q => q.Binding.Read(rows[q.Entity]),
                StringComparer.Ordinal
            );
        return (rows, answers);
    }

    /// <summary>
    /// Loads the options of every choice question. Saved answers are passed in so archived items
    /// already chosen stay offered.
    /// </summary>
    public async Task<Dictionary<string, IReadOnlyList<FormOption>>> LoadOptionsAsync(
        FormPage page,
        PageAnswers savedAnswers,
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
