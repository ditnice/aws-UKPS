using System.Text.Json;
using UKPS.Data.Enums;

namespace UKPS.Data.Entities.RecordWorkflow;

public class Record
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

public class RecordRevision
{
    public int Id { get; set; }
    public int RecordId { get; set; }

    /// <summary>
    /// Self-referencing FK. The revision this was branched from. Null for the first revision.
    /// </summary>
    public int? BasedOnRevisionId { get; set; }

    /// <summary>Global autoincrement per record.</summary>
    public int RevisionNo { get; set; }

    /// <summary>Increments on each new published revision e.g. 1 in 1.0</summary>
    public int MajorVersion { get; set; }

    /// <summary>Increments on each draft save within a major version e.g. 1 in 1.1</summary>
    public int MinorVersion { get; set; }

    public WorkflowStatus WorkflowStatus { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }

    // Navigation
    public Record Record { get; set; } = null!;
    public RecordRevision? BasedOnRevision { get; set; }
    public ICollection<RecordRevision> DerivedRevisions { get; set; } = [];
    public Identity.User CreatedByUser { get; set; } = null!;
    public Identity.User? UpdatedByUser { get; set; }
    public Identity.User? SubmittedByUser { get; set; }
    public ICollection<QaReview> QaReviews { get; set; } = [];
    public ICollection<RecordEvent> Events { get; set; } = [];
}

public class QaReview
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>How many times this major revision has been through QA.</summary>
    public int MajorRevisionSubmissionRoundNo { get; set; }

    public QaOutcome Outcome { get; set; }
    public string? Note { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime ReviewedAt { get; set; }

    // Navigation
    public RecordRevision Revision { get; set; } = null!;
    public Identity.User? ReviewedByUser { get; set; }
    public ICollection<QaReviewItem> QaReviewItems { get; set; } = [];
}

public class QaReviewItem
{
    public int Id { get; set; }
    public int QaReviewId { get; set; }

    /// <summary>section.field e.g. medicines_product_detail.indication</summary>
    public string FieldPath { get; set; } = null!;

    public IssueType IssueType { get; set; }
    public string? Note { get; set; }
    public ResolutionStatus ResolutionStatus { get; set; }
    public int? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation
    public QaReview QaReview { get; set; } = null!;
    public Identity.User? ResolvedByUser { get; set; }
}

public class RecordStatusHistory
{
    public int Id { get; set; }
    public int RecordId { get; set; }

    /// <summary>Null for the initial status set on record creation.</summary>
    public RecordStatus? FromStatus { get; set; }

    public RecordStatus ToStatus { get; set; }

    /// <summary>Null when moving back to active. Reasons to be validated throughout beta.</summary>
    public RecordStatusChangeReason? Reason { get; set; }

    public string? Note { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Record Record { get; set; } = null!;
    public Identity.User UpdatedByUser { get; set; } = null!;
}

public class RecordEvent
{
    public int Id { get; set; }
    public int RecordId { get; set; }

    /// <summary>Null for record-level events not tied to a specific revision.</summary>
    public int? RevisionId { get; set; }

    /// <summary>Populated for QA-related events.</summary>
    public int? QaReviewId { get; set; }

    /// <summary>Populated for issue-level events.</summary>
    public int? QaReviewItemId { get; set; }

    public RecordEventType EventType { get; set; }

    /// <summary>Null for system-triggered events.</summary>
    public int? PerformedBy { get; set; }

    public DateTime PerformedAt { get; set; }
    public string? Note { get; set; }

    /// <summary>Structured metadata specific to the event type. Stored as jsonb.</summary>
    public JsonDocument? Payload { get; set; }

    // Navigation
    public Record Record { get; set; } = null!;
    public RecordRevision? Revision { get; set; }
    public QaReview? QaReview { get; set; }
    public QaReviewItem? QaReviewItem { get; set; }
    public Identity.User? PerformedByUser { get; set; }
    public ICollection<RecordEventFieldChange> FieldChanges { get; set; } = [];
}

public class RecordEventFieldChange
{
    public int Id { get; set; }
    public int RecordEventId { get; set; }

    /// <summary>
    /// section.field convention e.g. medicines_product_detail.indication.
    /// Used for the progressive disclosure KPI — querying fields transitioning
    /// away from 'unknown' over time.
    /// </summary>
    public string FieldPath { get; set; } = null!;

    /// <summary>Null for newly populated fields.</summary>
    public string? OldValue { get; set; }

    /// <summary>Null for cleared fields.</summary>
    public string? NewValue { get; set; }

    // Navigation
    public RecordEvent RecordEvent { get; set; } = null!;
}
