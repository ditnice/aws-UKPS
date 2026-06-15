using System.Text.Json;
using UKPS.Api.Enums;

namespace UKPS.Api.Entities.RecordWorkflow;

internal sealed class RecordEvent
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
