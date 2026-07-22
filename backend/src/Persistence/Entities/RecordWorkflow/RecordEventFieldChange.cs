namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

internal sealed class RecordEventFieldChange
{
    public int Id { get; set; }
    public int RecordEventId { get; set; }

    /// <summary>
    /// section.field convention e.g. medicines_product_detail.indication.
    /// Used for the progressive disclosure KPI — querying fields transitioning
    /// away from 'unknown' over time.
    /// </summary>
    public required string FieldPath { get; set; }

    /// <summary>Null for newly populated fields.</summary>
    public string? OldValue { get; set; }

    /// <summary>Null for cleared fields.</summary>
    public string? NewValue { get; set; }

    // Navigation
    public RecordEvent? RecordEvent { get; set; }
}
