using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

internal sealed class Record
{
    public int Id { get; set; }
    public int OrganisationId { get; set; }
    public RecordType RecordType { get; set; }
    public RecordStatus RecordStatus { get; set; }

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
}
