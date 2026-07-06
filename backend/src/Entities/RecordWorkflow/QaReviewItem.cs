using UKPS.Api.Enums;

namespace UKPS.Api.Entities.RecordWorkflow;

internal sealed class QaReviewItem
{
    public int Id { get; set; }
    public int QaReviewId { get; set; }

    /// <summary>section.field e.g. medicines_product_detail.indication</summary>
    public required string FieldPath { get; set; }

    public IssueType IssueType { get; set; }
    public string? Note { get; set; }
    public ResolutionStatus ResolutionStatus { get; set; }
    public int? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation
    public QaReview? QaReview { get; set; }
    public Identity.User? ResolvedByUser { get; set; }
}
