using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

internal sealed class QaReview
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    public QaOutcome? Outcome { get; set; }
    public string? Note { get; set; }
    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public RecordRevision? Revision { get; set; }
    public Identity.User? ReviewedByUser { get; set; }
}
