namespace UKPS.Api.Entities.SharedRevisionContent;

internal sealed class OtherClinicalTrialNumber
{
    public int Id { get; set; }
    public int ClinicalTrialId { get; set; }

    /// <summary>e.g. ISRCTN, EudraCT</summary>
    public required string OtherRegistryNumber { get; set; }

    public int? DisplayOrder { get; set; }

    // Navigation
    public RecordClinicalTrial? ClinicalTrial { get; set; }
}
