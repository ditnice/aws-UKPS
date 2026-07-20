namespace UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

internal sealed class VaccinesPopulation
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public string? AgeGroup { get; set; }
    public string? RiskGroup { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
