using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
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

        FormPage page = loaded!.Page;
        var rows = await loader.LoadRowsAsync(
            page,
            loaded.Revision.Id,
            track: true,
            cancellationToken
        );
        Dictionary<string, FormAnswer> before = RecordFormPageLoader.ReadAnswers(page, rows);
        var options = await loader.LoadOptionsAsync(page, before, cancellationToken);

        var (after, errors) = PageAnswerValidator.Validate(page, command.Answers, options);
        if (errors.Count > 0)
        {
            return SaveRecordPageResult.Err(new SaveRecordPageError.Invalid(errors));
        }

        SaveRecordPageDto result = new() { NextPageId = loaded.Model.NextPage(page)?.Id };
        FormQuestion[] changed = page
            .Questions.Where(q => !after[q.Id].Equals(before[q.Id]))
            .ToArray();
        if (changed.Length == 0)
        {
            return SaveRecordPageResult.Ok(result);
        }

        foreach (FormQuestion question in changed)
        {
            object row = rows[question.Entity] ??= AddRow(question.Entity, loaded.Revision.Id);
            question.Binding.Write(dbContext, row, after[question.Id]);
        }

        await RecordChangeAsync(loaded, command, changed, before, after, cancellationToken);

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
