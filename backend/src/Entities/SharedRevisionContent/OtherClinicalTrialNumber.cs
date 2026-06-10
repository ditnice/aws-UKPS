using UKPS.Data.Enums;

namespace UKPS.Data.Entities.SharedRevisionContent;

public class OtherClinicalTrialNumber
{
    public int Id { get; set; }
    public int ClinicalTrialId { get; set; }

    /// <summary>e.g. ISRCTN, EudraCT</summary>
    public string OtherRegistryNumber { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    // Navigation
    public RecordClinicalTrial ClinicalTrial { get; set; } = null!;
}
