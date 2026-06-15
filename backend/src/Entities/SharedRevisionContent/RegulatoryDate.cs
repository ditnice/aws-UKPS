using UKPS.Api.Enums;

namespace UKPS.Api.Entities.SharedRevisionContent;

internal sealed class RegulatoryDate
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public DateEventType DateEvent { get; set; }
    public DatePrecision DatePrecision { get; set; }

    /// <summary>Quarter/Month stored as first day of that period.</summary>
    public DateOnly DateValue { get; set; }

    /// <summary>True for all estimated precision dates.</summary>
    public bool IsConfidential { get; set; }

    /// <summary>Relevant only for uk_licence and intl_licence events.</summary>
    public bool? ConditionalApprovalAnticipated { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}
