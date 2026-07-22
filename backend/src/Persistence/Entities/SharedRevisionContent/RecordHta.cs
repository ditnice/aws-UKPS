using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

internal sealed class RecordHta
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>Vaccines only: JCVI, NICE, or Not applicable (single-select radio).</summary>
    public string? HtaBodyVaccine { get; set; }

    /// <summary>Medicines only. Conditional on NICE being selected.</summary>
    public YesNoUnknown? HtaNiceAlignedPathway { get; set; }

    public string? HtaAdditionalDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ICollection<RecordHtaBody> HtaBodies { get; set; } = [];
}
