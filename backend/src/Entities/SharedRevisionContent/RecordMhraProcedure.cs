namespace UKPS.Api.Entities.SharedRevisionContent;

internal sealed class RecordMhraProcedure
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int? MhraProcedureTypeId { get; set; }
    public string? ProcedureDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.MhraProcedureType? MhraProcedureType { get; set; }
}
