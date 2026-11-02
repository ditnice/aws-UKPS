using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

internal sealed class Record
{
    public int Id { get; set; }
    public int OrganisationId { get; set; }
    public RecordType RecordType { get; set; }
    public RecordStatus RecordStatus { get; private set; }

    /// <summary>
    /// Immutable after insert. Timestamp of initial row creation / first draft.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }

    /// <summary>
    /// Last time the submitting organisation confirmed the record is current. Set on:
    ///   RecordPublished        -> the published revision's SubmittedToQa event time (the
    ///                             pharma submission timestamp, not the QA decision timestamp)
    ///   RecordReviewedNoChange -> the current time
    ///   RecordStatusChanged    -> the current time, when a user changes the status
    ///                             (not when the record is archived automatically)
    /// Not touched by any other event, including QA rejection (QA tracks data
    /// validity, not currency).
    /// Next review due:
    ///   medicine + published -> reviewed_at + 3 months
    ///   medicine + on_hold   -> reviewed_at + 6 months
    ///   vaccine + published  -> reviewed_at + 6 months
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public Identity.Organisation? Organisation { get; set; }
    public Identity.User? CreatedByUser { get; set; }
    public ICollection<RecordRevision> Revisions { get; set; } = [];
    public ICollection<RecordStatusHistory> StatusHistory { get; set; } = [];
    public ICollection<RecordEvent> Events { get; set; } = [];

    /// <summary>
    /// Gets the actions a user can take on a record, in <see cref="RecordAction"/> order.
    /// </summary>
    /// <param name="state">The record's status and its latest revision's workflow status.</param>
    /// <param name="hasSingleRevision">Whether the record has only one revision.</param>
    /// <returns>The permitted actions.</returns>
    internal static IReadOnlyCollection<RecordAction> GetPermittedActions(
        RecordLifecycleState state,
        bool hasSingleRevision
    )
    {
        return new RecordLifecycleStateMachine(state)
            .GetPermittedCommands()
            .Where(x => x != RecordLifecycleStateMachine.Command.Discard || hasSingleRevision)
            .Select(x => ConvertCommandToAction(x, state.RecordStatus))
            .OfType<RecordAction>()
            .Append(RecordAction.ViewTimeline)
            .Distinct()
            .Order()
            .ToArray();
    }

    private static RecordAction? ConvertCommandToAction(
        RecordLifecycleStateMachine.Command command,
        RecordStatus recordStatus
    )
    {
        return command switch
        {
            RecordLifecycleStateMachine.Command.EditDraft
            or RecordLifecycleStateMachine.Command.CreateRevision => RecordAction.Update,
            RecordLifecycleStateMachine.Command.PutOnHold
            or RecordLifecycleStateMachine.Command.Resume
            or RecordLifecycleStateMachine.Command.Archive => RecordAction.ChangeStatus,
            RecordLifecycleStateMachine.Command.ConfirmCurrent => recordStatus
            == RecordStatus.OnHold
                ? RecordAction.ConfirmOnHold
                : RecordAction.ConfirmCurrent,
            RecordLifecycleStateMachine.Command.Discard => RecordAction.Discard,
            _ => null,
        };
    }

    /// <summary>Changes the record's status and records the change in its status history.</summary>
    /// <param name="toStatus">The new status.</param>
    /// <param name="time">When the status changed.</param>
    /// <param name="changedBy">The user who changed it, or null for system-triggered changes.</param>
    /// <param name="reason">Why the status changed.</param>
    /// <param name="note">Further details about the change.</param>
    internal void ChangeStatus(
        RecordStatus toStatus,
        DateTime time,
        User? changedBy,
        RecordStatusChangeReason? reason = null,
        string? note = null
    )
    {
        StatusHistory.Add(
            new RecordStatusHistory
            {
                Record = this,
                FromStatus = RecordStatus,
                ToStatus = toStatus,
                Reason = reason,
                Note = note,
                UpdatedAt = time,
                UpdatedByUser = changedBy,
            }
        );
        RecordStatus = toStatus;
    }

    internal static (Record record, RecordRevision recordRevision) CreateInitial(
        Organisation organisation,
        DateTime time,
        User currentUser
    )
    {
        Record record = new Record()
        {
            OrganisationId = organisation.Id,
            RecordType = RecordType.Medicine,
            CreatedAt = time,
            CreatedByUser = currentUser,
            RecordStatus = RecordStatus.Unpublished,
            StatusHistory =
            [
                new RecordStatusHistory()
                {
                    FromStatus = null,
                    ToStatus = RecordStatus.Unpublished,
                    UpdatedAt = time,
                    UpdatedByUser = currentUser,
                },
            ],
        };
        RecordRevision revision = new RecordRevision()
        {
            CreatedAt = time,
            CreatedByUser = currentUser,
            WorkflowStatus = WorkflowStatus.Draft,
            Record = record,
        };
        record.Events.Add(
            new RecordEvent()
            {
                Revision = revision,
                EventType = RecordEventType.RecordCreated,
                PerformedAt = time,
                PerformedByUser = currentUser,
            }
        );

        return (record, revision);
    }

    /// <summary>
    /// Discards the record and its latest revision, and adds the timeline event. A record can only
    /// be discarded while its only revision is a draft.
    /// </summary>
    /// <param name="latestRevision">The record's latest revision.</param>
    /// <param name="hasSingleRevision">Whether the latest revision is the record's only one.</param>
    /// <param name="time">When the record was discarded.</param>
    /// <param name="discardedBy">The user who discarded it, or null when discarded automatically.</param>
    /// <param name="reason">Why the record was discarded.</param>
    /// <returns>Whether the record was discarded. Nothing is changed when it can't be.</returns>
    internal bool TryDiscard(
        RecordRevision latestRevision,
        bool hasSingleRevision,
        DateTime time,
        User? discardedBy,
        RecordStatusChangeReason reason
    )
    {
        if (!hasSingleRevision)
        {
            return false;
        }

        var result = new RecordLifecycleStateMachine(
            new(RecordStatus, latestRevision.WorkflowStatus)
        ).TrySendCommand(RecordLifecycleStateMachine.Command.Discard);
        if (!result.Success)
        {
            return false;
        }

        latestRevision.WorkflowStatus = result.CurrentState.LatestRevisionStatus;
        ChangeStatus(result.CurrentState.RecordStatus, time, discardedBy, reason);
        Events.Add(
            new RecordEvent
            {
                Record = this,
                Revision = latestRevision,
                EventType = RecordEventType.RecordStatusChanged,
                PerformedAt = time,
                PerformedByUser = discardedBy,
            }
        );
        return true;
    }
}
