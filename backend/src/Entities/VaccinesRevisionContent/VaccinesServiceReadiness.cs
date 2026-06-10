using UKPS.Data.Enums;

namespace UKPS.Data.Entities.VaccinesRevisionContent;

public class VaccinesServiceReadiness
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int StorageRequirementId { get; set; }
    public YesNoNotYetConfirmed RequiresReconstitution { get; set; }
    public string? AdditionalServiceNotes { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.VaccineStorageRequirement StorageRequirement { get; set; } = null!;
}
