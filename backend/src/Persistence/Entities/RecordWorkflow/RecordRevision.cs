using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

internal sealed class RecordRevision
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
    public Record? Record { get; set; }
    public RecordRevision? BasedOnRevision { get; set; }
    public ICollection<RecordRevision> DerivedRevisions { get; set; } = [];
    public Identity.User? CreatedByUser { get; set; }
    public Identity.User? UpdatedByUser { get; set; }
    public Identity.User? SubmittedByUser { get; set; }
    public ICollection<QaReview> QaReviews { get; set; } = [];
    public ICollection<RecordEvent> Events { get; set; } = [];
}
