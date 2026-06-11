using UKPS.Api.Enums;

namespace UKPS.Api.Entities.RecordWorkflow;

internal sealed class QaReview
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
