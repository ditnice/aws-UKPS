namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

internal sealed class RecordMhraProcedure
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int? MhraProcedureTypeId { get; set; }

    /// <summary>
    /// FK to IrpReferenceRegulator. Populated when MhraProcedureType = IRP.
    /// </summary>
    public int? IrpReferenceRegulatorId { get; set; }
    public string? ProcedureDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.MhraProcedureType? MhraProcedureType { get; set; }
    public ReferenceData.IrpReferenceRegulator? IrpReferenceRegulator { get; set; }
}
