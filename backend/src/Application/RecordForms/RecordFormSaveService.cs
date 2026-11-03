using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Rules;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
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
        (ModelledPageUpdated? modelledPageUpdate, SaveRecordPageError? updateModelError) =
            await ValidateCommandAndModelUpdate(currentPage!, command, cancellationToken);
        if (updateModelError is not null)
        {
            return SaveRecordPageResult.Err(updateModelError);
        }
        if (modelledPageUpdate is null)
        {
            return CreateSuccessResult(currentPage!);
        }

        (bool updateRequired, SaveRecordPageResult result) = await ApplyUpdates(
            modelledPageUpdate!,
            currentPage!,
            cancellationToken
        );
        if (!updateRequired)
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

    async Task<(
        ModelledPageUpdated? updatedPage,
        SaveRecordPageError? error
    )> ValidateCommandAndModelUpdate(
        LoadedPage currentPage,
        SaveRecordPageCommand command,
        CancellationToken cancellationToken
    )
    {
        FormPage page = currentPage.Page;
        Dictionary<FormEntity, object?> rows = await loader.LoadRowsAsync(
            page,
            currentPage.Revision.Id,
            track: true,
            cancellationToken
        );
        PageAnswers before = RecordFormPageLoader.ReadAnswers(page, rows);
        Dictionary<string, IReadOnlyList<FormOption>> options = await loader.LoadOptionsAsync(
            page,
            before,
            cancellationToken
        );
        (PageAnswers after, Dictionary<string, string>? errors) = PageAnswerValidator.Validate(
            page,
            command.Answers,
            options
        );

        if (before.PageHashIsEqual(after))
        {
            return (null, null); // No changes were made, so no error is returned.
        }

        if (!before.PageHashIsEqual(command.PageHash))
        {
            return (null, new SaveRecordPageError.PageAnswersChanged());
        }

        if (errors.Count > 0)
        {
            return (null, new SaveRecordPageError.Invalid(errors));
        }

        return (new(before, after, rows), null);
    }

    private async Task<(bool updateRequired, SaveRecordPageResult)> ApplyUpdates(
        ModelledPageUpdated modelledPageUpdate,
        LoadedPage currentPage,
        CancellationToken cancellationToken
    )
    {
        FormQuestion[] changed = currentPage
            .Page.Questions.Where(q =>
                !modelledPageUpdate.After[q.Id].Equals(modelledPageUpdate.Before[q.Id])
            )
            .ToArray();
        if (changed.Length == 0)
        {
            return (false, CreateSuccessResult(currentPage));
        }

        foreach (FormQuestion question in changed)
        {
            object row = modelledPageUpdate.Rows[question.Entity] ??= AddRow(
                question.Entity,
                currentPage.Revision.Id
            );
            question.Binding.Write(dbContext, row, modelledPageUpdate.After[question.Id]);
        }

        await RecordChangeAsync(
            currentPage,
            changed,
            modelledPageUpdate.Before,
            modelledPageUpdate.After,
            cancellationToken
        );

        return (true, CreateSuccessResult(currentPage));
    }

    private static SaveRecordPageResult CreateSuccessResult(LoadedPage currentPage)
    {
        return SaveRecordPageResult.Ok(
            new SaveRecordPageDto { NextPageId = currentPage.Model.NextPage(currentPage.Page)?.Id }
        );
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

        if (loaded.Revision.WorkflowStatus != WorkflowStatus.Draft)
        {
            return new SaveRecordPageError.NotDraft();
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
        PageAnswers before,
        PageAnswers after,
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

    private sealed record ModelledPageUpdated(
        PageAnswers Before,
        PageAnswers After,
        Dictionary<FormEntity, object?> Rows
    );
}
