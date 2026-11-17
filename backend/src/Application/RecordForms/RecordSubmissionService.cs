using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Application.RecordForms;

internal sealed class RecordSubmissionService(
    AppDbContext dbContext,
    RecordFormPageLoader loader,
    RevisionChecker revisionChecker,
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

        // The hash is checked before the errors, so a user who hasn't seen the latest answers
        // reloads straight away rather than fixing errors in answers that have since changed.
        LoadedRevision revision = await revisionChecker.LoadAsync(form, cancellationToken);
        if (!string.Equals(command.RevisionHash, revision.Hash, StringComparison.Ordinal))
        {
            return SubmitRecordResult.Err(new SubmitRecordError.RecordChanged());
        }

        // Checked again even when the hash matches, as date rules change with the date.
        RevisionCheck check = await revisionChecker.CheckAsync(form, revision, cancellationToken);
        if (check.Errors.Count > 0)
        {
            return SubmitRecordResult.Err(new SubmitRecordError.Invalid(check.Errors));
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
        switch (revisionChecker.FindSubmitBlocker(form))
        {
            case SubmitBlocker.CannotSubmit:
                return new SubmitRecordError.CannotSubmit();
            case SubmitBlocker.NotSubmittable:
                return new SubmitRecordError.NotSubmittable();
        }

        if (!string.Equals(command.FormVersion, form.Model.Version, StringComparison.Ordinal))
        {
            return new SubmitRecordError.FormVersionChanged();
        }

        return null;
    }
}
