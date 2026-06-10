using UKPS.Data.Enums;

namespace UKPS.Data.Entities.RecordWorkflow;

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
