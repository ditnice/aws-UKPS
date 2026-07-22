using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

internal sealed class RecordStatusHistory
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
    public Record? Record { get; set; }
    public Identity.User? UpdatedByUser { get; set; }
}
