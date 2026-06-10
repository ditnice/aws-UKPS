using UKPS.Data.Enums;

namespace UKPS.Data.Entities.VaccinesRevisionContent;

public class VaccinesPopulation
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public string? AgeGroup { get; set; }
    public string? RiskGroup { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}
