using UKPS.Api.Enums;

namespace UKPS.Api.Entities.RecordWorkflow;

internal sealed class Record
{
    public int Id { get; set; }
    public int OrganisationId { get; set; }
    public RecordType RecordType { get; set; }
    public RecordStatus RecordStatus { get; set; }

    /// <summary>
    /// FK to the current live published revision. Null until first publication.
    /// Both revision FKs are nullable to allow the Record row to be inserted
    /// before the first RecordRevision row is created.
    /// </summary>
    public int? PublishedRevisionId { get; set; }

    /// <summary>
    /// FK to the active draft revision, if one exists. Null when no draft is in progress.
    /// </summary>
    public int? CurrentDraftRevisionId { get; set; }

    /// <summary>
    /// Immutable after insert. Timestamp of initial row creation / first draft.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }

    /// <summary>
    /// Last time the submitting organisation confirmed the record is current.
    /// Updated on revision_published and record_reviewed_no_change events only.
    /// NOT updated on QA approval (QA tracks data validity, not currency).
    /// Next review due:
    ///   medicine + active  -> reviewed_at + 3 months
    ///   medicine + on_hold -> reviewed_at + 6 months
    ///   vaccine + active   -> reviewed_at + 6 months
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public Identity.Organisation Organisation { get; set; } = null!;
    public Identity.User? CreatedByUser { get; set; }
    public RecordRevision? PublishedRevision { get; set; }
    public RecordRevision? CurrentDraftRevision { get; set; }
    public ICollection<RecordRevision> Revisions { get; set; } = [];
    public ICollection<RecordStatusHistory> StatusHistory { get; set; } = [];
    public ICollection<RecordEvent> Events { get; set; } = [];
}
