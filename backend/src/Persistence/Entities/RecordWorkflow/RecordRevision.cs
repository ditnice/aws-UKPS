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
