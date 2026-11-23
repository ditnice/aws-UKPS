using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using ChangeRecordStatusResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Records.Errors.ChangeRecordStatusError>;

namespace UKPS.Api.Application.Records;

internal sealed class RecordStatusService(
    AppDbContext dbContext,
    IDateTimeProvider timeProvider,
    IOrganisationAuthoriser organisationAuthoriser,
    ICurrentUserInfoService currentUserInfoService,
    CurrentDbUserEntityService currentDbUserEntityService
) : IRecordStatusService
{
    /// <summary>
    /// The reasons a user can give for each status change. Reasons the system gives, such as
    /// <see cref="RecordStatusChangeReason.ArchivedAutomatically"/>, are never accepted.
    /// </summary>
    private static readonly Dictionary<
        RecordStatusChangeAction,
        RecordStatusChangeReason[]
    > _reasons = new()
    {
        [RecordStatusChangeAction.PutOnHold] =
        [
            RecordStatusChangeReason.TrialSuspended,
            RecordStatusChangeReason.FilingWithdrawn,
            RecordStatusChangeReason.Other,
        ],
        [RecordStatusChangeAction.Resume] =
        [
            RecordStatusChangeReason.TrialResumed,
            RecordStatusChangeReason.PlansRefiled,
            RecordStatusChangeReason.Other,
        ],
        [RecordStatusChangeAction.Archive] =
        [
            RecordStatusChangeReason.DevelopmentDiscontinued,
            RecordStatusChangeReason.Other,
        ],
    };

    public async Task<ChangeRecordStatusResult> ChangeStatus(
        int recordId,
        int organisationId,
        RecordStatusChangeAction change,
        ChangeRecordStatusCommand command,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(command);
        currentUserInfoService.ThrowIfOrganisationNotSelected(organisationId);

        if (command.Reason is not { } reason || !_reasons[change].Contains(reason))
        {
            return ChangeRecordStatusResult.Err(new ChangeRecordStatusError.InvalidReason());
        }

        string? note = string.IsNullOrWhiteSpace(command.Note) ? null : command.Note.Trim();
        // Other is given with the detail of the reason.
        if (reason == RecordStatusChangeReason.Other && note is null)
        {
            return ChangeRecordStatusResult.Err(new ChangeRecordStatusError.NoteRequired());
        }

        Result<LoadedRecord, ChangeRecordStatusError> loaded = await LoadRecord(
            recordId,
            organisationId,
            cancellationToken
        );
        if (!loaded.IsOk)
        {
            return ChangeRecordStatusResult.Err(loaded.Error!);
        }

        (Record record, RecordRevision latestRevision) = loaded.Value;
        User currentUser = await currentDbUserEntityService.GetCurrentUser(cancellationToken);
        bool isChanged = record.TryChangeStatusByUser(
            ToLifecycleCommand(change),
            latestRevision,
            timeProvider.GetUtcNow(),
            currentUser,
            reason,
            note
        );
        if (!isChanged)
        {
            return ChangeRecordStatusResult.Err(
                new ChangeRecordStatusError.TransitionNotPermitted()
            );
        }

        return await Save(latestRevision, cancellationToken);
    }

    /// <summary>
    /// Loads the record and its latest revision for tracking, once the caller is authorised.
    /// </summary>
    private async Task<Result<LoadedRecord, ChangeRecordStatusError>> LoadRecord(
        int recordId,
        int organisationId,
        CancellationToken cancellationToken
    )
    {
        // Authorise against the requested organisation before looking the record up, so an
        // unauthorised caller gets the same response whether or not the record exists.
        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Read,
                organisationId
            )
        )
        {
            return Result<LoadedRecord, ChangeRecordStatusError>.Err(
                new ChangeRecordStatusError.NotAllowed()
            );
        }

        // One query, so the record and its latest revision are read at the same moment.
        var loaded = await dbContext
            .Records.Where(r => r.Id == recordId && r.OrganisationId == organisationId)
            .Select(r => new
            {
                Record = r,
                LatestRevision = r
                    .Revisions.OrderByDescending(rev => rev.CreatedAt)
                    .ThenByDescending(rev => rev.Id)
                    .FirstOrDefault(),
            })
            .SingleOrDefaultAsync(cancellationToken);

        // Every record is created with a revision, so a missing one only guards against bad data.
        if (loaded?.LatestRevision is null)
        {
            return Result<LoadedRecord, ChangeRecordStatusError>.Err(
                new ChangeRecordStatusError.NotFound()
            );
        }

        return organisationAuthoriser.CanPerformOperationOnOrganisation(
            Operation.ChangeRecordStatus,
            organisationId
        )
            ? Result<LoadedRecord, ChangeRecordStatusError>.Ok(
                new LoadedRecord(loaded.Record, loaded.LatestRevision)
            )
            : Result<LoadedRecord, ChangeRecordStatusError>.Err(
                new ChangeRecordStatusError.CannotChangeStatus()
            );
    }

    private async Task<ChangeRecordStatusResult> Save(
        RecordRevision latestRevision,
        CancellationToken cancellationToken
    )
    {
        // The latest revision's status is part of the state the change was checked against, so
        // its row is updated, checked against the version read with it. Anything that changed it
        // since, such as a submission, a content save or another status change, fails the save
        // and nothing is written.
        dbContext.Entry(latestRevision).Property(r => r.WorkflowStatus).IsModified = true;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ChangeRecordStatusResult.Err(new ChangeRecordStatusError.ConcurrencyError());
        }

        return ChangeRecordStatusResult.Ok();
    }

    private static RecordLifecycleStateMachine.Command ToLifecycleCommand(
        RecordStatusChangeAction change
    ) =>
        change switch
        {
            RecordStatusChangeAction.PutOnHold => RecordLifecycleStateMachine.Command.PutOnHold,
            RecordStatusChangeAction.Resume => RecordLifecycleStateMachine.Command.Resume,
            RecordStatusChangeAction.Archive => RecordLifecycleStateMachine.Command.Archive,
            _ => throw new UnreachableException($"Unhandled record status change {change}."),
        };

    private sealed record LoadedRecord(Record Record, RecordRevision LatestRevision);
}
