namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

/// <summary>A single field changed by a save to a revision.</summary>
internal sealed class RecordFieldAudit
{
    public int Id { get; set; }

    /// <summary>Denormalised from the revision to avoid joining through it.</summary>
    public int RecordId { get; set; }

    public int RevisionId { get; set; }

    /// <summary>
    /// section.field convention e.g. medicines_indication_detail.indication.
    /// </summary>
    public required string FieldPath { get; set; }

    /// <summary>Null for newly populated fields.</summary>
    public string? OldValue { get; set; }

    /// <summary>Null for cleared fields.</summary>
    public string? NewValue { get; set; }

    public int ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }

    // Navigation
    public Record? Record { get; set; }
    public RecordRevision? Revision { get; set; }
    public Identity.User? ChangedByUser { get; set; }
}
