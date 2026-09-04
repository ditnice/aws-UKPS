namespace UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

internal sealed class VaccinesServiceReadiness
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int StorageRequirementId { get; set; }

    /// <summary>
    /// Conditional on StorageRequirementId referencing the 'Other' option.
    /// Free text description of storage requirements not covered by standard options.
    /// </summary>
    public string? StorageRequirementOther { get; set; }

    /// <summary>
    /// Mandatory. CiC. Covers number of doses, interval, schedule variation by
    /// age/risk group, and anticipated booster doses and timing if known.
    /// </summary>
    public required string DosingSchedule { get; set; }

    public string? AdditionalServiceNotes { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.VaccineStorageRequirement? StorageRequirement { get; set; }
}
