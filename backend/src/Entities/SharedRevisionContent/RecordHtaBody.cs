namespace UKPS.Api.Entities.SharedRevisionContent;

/// <summary>
/// HTA bodies selected for a medicine record.
/// Stored as label varchar (NICE, SMC, AWMSG) rather than FK to organisation —
/// HTA bodies are a small, stable set that do not require the full organisation model.
/// </summary>
internal sealed class RecordHtaBody
{
    public int RecordHtaId { get; set; }

    /// <summary>e.g. NICE, SMC, AWMSG</summary>
    public string Label { get; set; } = null!;

    // Navigation
    public RecordHta RecordHta { get; set; } = null!;
}
