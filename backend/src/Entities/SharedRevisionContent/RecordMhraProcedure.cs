using UKPS.Api.Enums;

namespace UKPS.Api.Entities.SharedRevisionContent;

public class RecordMhraProcedure
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int? MhraProcedureTypeId { get; set; }
    public string? ProcedureDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.MhraProcedureType? MhraProcedureType { get; set; }
}
