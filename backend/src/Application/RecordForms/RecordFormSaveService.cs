using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Rules;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms;

internal sealed class RecordFormSaveService(
    AppDbContext dbContext,
    RecordFormPageLoader loader,
    CurrentDbUserEntityService currentDbUserEntityService,
    IDateTimeProvider dateTimeProvider
) : IRecordFormSaveService
{
    public async Task<SaveRecordPageResult> SavePage(
        int recordId,
        string pageId,
        SaveRecordPageCommand command,
        CancellationToken cancellationToken
    )
    {
        (LoadedPage? currentPage, SaveRecordPageError? loadError) = await LoadPage(
            recordId,
            pageId,
            command,
            cancellationToken
        );
        if (loadError is not null)
        {
            return SaveRecordPageResult.Err(loadError);
        }

        (ModelledPageUpdate? update, SaveRecordPageError? updateModelError) =
            await ValidateCommandAndModelUpdate(currentPage!, command, cancellationToken);
        if (updateModelError is not null)
        {
            return SaveRecordPageResult.Err(updateModelError);
        }

        SaveRecordPageResult result = CreateSuccessResult(currentPage!, update!.Applicability);
        if (!await ApplyUpdates(update, currentPage!, cancellationToken))
        {
            return result;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return SaveRecordPageResult.Err(new SaveRecordPageError.ConcurrentUpdate());
        }

        return result;
    }

    private async Task<(LoadedPage? currentPage, SaveRecordPageError? error)> LoadPage(
        int recordId,
        string pageId,
        SaveRecordPageCommand command,
        CancellationToken cancellationToken
    )
    {
        (LoadedPage? loaded, PageLookupFailure failure) = await loader.LoadPageAsync(
            recordId,
            pageId,
            track: true,
            cancellationToken
        );
        SaveRecordPageError? error = CheckCanSave(loaded, failure, command);
        return (loaded, error);
    }

    /// <summary>
    /// Validates the submitted page and models the revision's answers after the save. A page with
    /// no change gives an update whose answers are unchanged, whatever its hash.
    /// </summary>
    private async Task<(
        ModelledPageUpdate? update,
        SaveRecordPageError? error
    )> ValidateCommandAndModelUpdate(
        LoadedPage currentPage,
        SaveRecordPageCommand command,
        CancellationToken cancellationToken
    )
    {
        LoadedAnswers? saved = await loader.LoadAnswersAsync(
            currentPage,
            forSave: true,
            cancellationToken
        );
        if (saved is null)
        {
            return (null, new SaveRecordPageError.NotFound());
        }

        FormModel model = currentPage.Model;
        FormPage page = currentPage.Page;
        Dictionary<string, FormAnswer> before = saved.Answers;
        PageAnswers pageBefore = PageAnswers.For(page, before);
        var options = await loader.LoadOptionsAsync(page, pageBefore, cancellationToken);

        var (parsed, errors) = PageAnswerValidator.Parse(page, command.Answers);
        Dictionary<string, FormAnswer> submitted = Overlay(before, parsed);
        Applicability applicability = FormApplicability.Evaluate(model, submitted);
        PageAnswerValidator.Check(page, parsed, options, applicability, errors);

        // Answers that no longer apply, on this page or any page that depends on it, are cleared
        // in the same save, so stored answers always match what applies.
        Dictionary<string, FormAnswer> after = FormApplicability.ClearInapplicable(
            model.AffectedBy(page),
            submitted,
            applicability
        );

        // Answers that failed to parse or validate are left out, so a page with errors never
        // matches the stored answers and its errors are reported.
        PageAnswers pageAfter = PageAnswers.For(
            page,
            after
                .Where(a => parsed.ContainsKey(a.Key) && !errors.ContainsKey(a.Key))
                .ToDictionary(StringComparer.Ordinal)
        );
        if (pageBefore.PageHashIsEqual(pageAfter))
        {
            return (new(before, before, saved.Rows, applicability), null);
        }

        if (!pageBefore.PageHashIsEqual(command.PageHash))
        {
            return (null, new SaveRecordPageError.PageAnswersChanged());
        }

        if (errors.Count > 0)
        {
            return (null, new SaveRecordPageError.Invalid(errors));
        }

        return (new(before, after, saved.Rows, applicability), null);
    }

    /// <summary>
    /// Writes every loaded answer that differs, adding missing rows, and records the change.
    /// Returns false when nothing changed.
    /// </summary>
    private async Task<bool> ApplyUpdates(
        ModelledPageUpdate update,
        LoadedPage currentPage,
        CancellationToken cancellationToken
    )
    {
        FormQuestion[] changed = currentPage
            .Model.Questions.Where(q =>
                update.Before.TryGetValue(q.Id, out FormAnswer? old)
                && !update.After[q.Id].Equals(old)
            )
            .ToArray();
        if (changed.Length == 0)
        {
            return false;
        }

        foreach (FormQuestion question in changed)
        {
            object row = update.Rows[question.Entity] ??= AddRow(
                question.Entity,
                currentPage.Revision.Id
            );
            question.Binding.Write(dbContext, row, update.After[question.Id]);
        }

        await RecordChangeAsync(
            currentPage,
            changed,
            update.Before,
            update.After,
            cancellationToken
        );
        return true;
    }

    private static SaveRecordPageResult CreateSuccessResult(
        LoadedPage currentPage,
        Applicability applicability
    ) =>
        SaveRecordPageResult.Ok(
            new SaveRecordPageDto
            {
                NextPageId = currentPage.Model.NextPage(currentPage.Page, applicability)?.Id,
            }
        );

    private static Dictionary<string, FormAnswer> Overlay(
        IReadOnlyDictionary<string, FormAnswer> answers,
        IReadOnlyDictionary<string, FormAnswer> overrides
    )
    {
        Dictionary<string, FormAnswer> result = new(answers, StringComparer.Ordinal);
        foreach (var (questionId, answer) in overrides)
        {
            result[questionId] = answer;
        }

        return result;
    }

    private static SaveRecordPageError? CheckCanSave(
        LoadedPage? loaded,
        PageLookupFailure failure,
        SaveRecordPageCommand command
    )
    {
        if (loaded is null)
        {
            return failure == PageLookupFailure.NotAllowed
                ? new SaveRecordPageError.NotAllowed()
                : new SaveRecordPageError.NotFound();
        }

        if (!loaded.CanEdit)
        {
            return new SaveRecordPageError.CannotEdit();
        }

        if (!loaded.Record.CanEditDraft(loaded.Revision))
        {
            return new SaveRecordPageError.NotEditable();
        }

        if (!string.Equals(command.FormVersion, loaded.Model.Version, StringComparison.Ordinal))
        {
            return new SaveRecordPageError.FormVersionChanged();
        }

        return null;
    }

    private object AddRow(FormEntity entity, int revisionId)
    {
        if (entity.RowSource != FormRowSource.FirstSave)
        {
            throw new InvalidOperationException(
                $"Revision {revisionId} has no {entity.Name} row, which record creation should have inserted."
            );
        }

        return entity.AddRow(dbContext, revisionId);
    }

    /// <summary>
    /// Touches the revision, so its concurrency token moves on, and writes a field audit row per
    /// changed question.
    /// </summary>
    private async Task RecordChangeAsync(
        LoadedPage loaded,
        FormQuestion[] changed,
        Dictionary<string, FormAnswer> before,
        Dictionary<string, FormAnswer> after,
        CancellationToken cancellationToken
    )
    {
        DateTime time = dateTimeProvider.GetUtcNow();
        User user = await currentDbUserEntityService.GetCurrentUser(cancellationToken);
        RecordRevision revision = loaded.Revision;

        // Update the revision row, checked against the version the page was loaded at, so a save
        // that lands in between is caught as a concurrency conflict. No revision column changes on
        // a content save, so an unchanged column is marked modified to force the UPDATE, which
        // moves xmin on.
        EntityEntry<RecordRevision> entry = dbContext.Entry(revision);
        entry.Property(static r => r.WorkflowStatus).IsModified = true;

        dbContext.RecordFieldAudits.AddRange(
            changed.Select(q => new RecordFieldAudit
            {
                RecordId = loaded.Record.Id,
                RevisionId = revision.Id,
                FieldPath = q.Id,
                OldValue = before[q.Id].ToAuditText(),
                NewValue = after[q.Id].ToAuditText(),
                ChangedBy = user.Id,
                ChangedAt = time,
            })
        );
    }

    /// <summary>The revision's loaded answers before and after the save, and what applies after.</summary>
    private sealed record ModelledPageUpdate(
        Dictionary<string, FormAnswer> Before,
        Dictionary<string, FormAnswer> After,
        Dictionary<FormEntity, object?> Rows,
        Applicability Applicability
    );
}
