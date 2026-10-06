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
    /// Last time the submitting organisation confirmed the record is current.
    /// Set from the triggering revision's SubmittedAt (the pharma submission
    /// timestamp, not the QA reviewer's decision timestamp) on RecordPublished,
    /// and to the current time on RecordReviewedNoChange. Not touched by any
    /// other event, including QA rejection (QA tracks data validity, not
    /// currency).
    /// Next review due:
    ///   medicine + active  -> reviewed_at + 3 months
    ///   medicine + on_hold -> reviewed_at + 6 months
    ///   vaccine + active   -> reviewed_at + 6 months
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public Identity.Organisation? Organisation { get; set; }
    public Identity.User? CreatedByUser { get; set; }
    public ICollection<RecordRevision> Revisions { get; set; } = [];
    public ICollection<RecordStatusHistory> StatusHistory { get; set; } = [];
    public ICollection<RecordEvent> Events { get; set; } = [];

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
}
