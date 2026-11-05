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
        var (loaded, failure) = await loader.LoadPageAsync(
            recordId,
            pageId,
            track: true,
            cancellationToken
        );
        SaveRecordPageError? error = CheckCanSave(loaded, failure, command);
        if (error is not null)
        {
            return SaveRecordPageResult.Err(error);
        }

        PageAnswers? saved = await loader.LoadAnswersAsync(
            loaded!,
            forSave: true,
            cancellationToken
        );
        if (saved is null)
        {
            return SaveRecordPageResult.Err(new SaveRecordPageError.NotFound());
        }

        FormModel model = loaded!.Model;
        FormPage page = loaded.Page;
        Dictionary<string, FormAnswer> before = saved.Answers;
        var options = await loader.LoadOptionsAsync(page, before, cancellationToken);
        var (pageAnswers, errors) = PageAnswerValidator.Parse(page, command.Answers);
        Dictionary<string, FormAnswer> submitted = Overlay(before, pageAnswers);
        Applicability applicability = FormApplicability.Evaluate(model, submitted);
        PageAnswerValidator.Check(page, pageAnswers, options, applicability, errors);
        if (errors.Count > 0)
        {
            return SaveRecordPageResult.Err(new SaveRecordPageError.Invalid(errors));
        }

        // Answers that no longer apply, on this page or any page that depends on it, are cleared
        // in the same save, so stored answers always match what applies.
        Dictionary<string, FormAnswer> after = FormApplicability.ClearInapplicable(
            model.AffectedBy(page),
            submitted,
            applicability
        );
        SaveRecordPageDto result = new() { NextPageId = model.NextPage(page, applicability)?.Id };
        FormQuestion[] changed = WriteChanges(model, saved.Rows, loaded.Revision.Id, before, after);
        if (changed.Length == 0)
        {
            return SaveRecordPageResult.Ok(result);
        }

        await RecordChangeAsync(loaded, command, changed, before, after, cancellationToken);
        return await SaveChangesAsync(result, cancellationToken);
    }

    /// <summary>
    /// Writes every loaded answer that differs from <paramref name="before"/>, adding missing rows,
    /// and returns the questions written.
    /// </summary>
    private FormQuestion[] WriteChanges(
        FormModel model,
        Dictionary<FormEntity, object?> rows,
        int revisionId,
        Dictionary<string, FormAnswer> before,
        Dictionary<string, FormAnswer> after
    )
    {
        FormQuestion[] changed = model
            .Questions.Where(q =>
                before.TryGetValue(q.Id, out FormAnswer? old) && !after[q.Id].Equals(old)
            )
            .ToArray();
        foreach (FormQuestion question in changed)
        {
            object row = rows[question.Entity] ??= AddRow(question.Entity, revisionId);
            question.Binding.Write(dbContext, row, after[question.Id]);
        }

        return changed;
    }

    private async Task<SaveRecordPageResult> SaveChangesAsync(
        SaveRecordPageDto result,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return SaveRecordPageResult.Err(new SaveRecordPageError.RevisionChanged());
        }

        return SaveRecordPageResult.Ok(result);
    }

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

        return command.RevisionVersion != loaded.Revision.Version
            ? new SaveRecordPageError.RevisionChanged()
            : null;
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
        SaveRecordPageCommand command,
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
        entry.Property(static r => r.Version).OriginalValue = command.RevisionVersion;
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
}
