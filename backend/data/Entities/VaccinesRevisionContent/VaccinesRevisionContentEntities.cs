using UKPS.Data.Enums;

namespace UKPS.Data.Entities.VaccinesRevisionContent;

public class VaccinesProductDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>
    /// Short human-readable label for the homepage.
    /// Guidance: use disease target + population pattern e.g. RSV — adults 60+.
    /// </summary>
    public string RecordTitle { get; set; } = null!;

    /// <summary>
    /// Internal code or working name e.g. mRNA-1273, BNT162b2, V116.
    /// Primary identifier at pipeline stage. Also known as candidate name in industry.
    /// </summary>
    public string CompanyCode { get; set; } = null!;

    public string? BrandedName { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ICollection<VaccinesCompanyCode> CompanyCodes { get; set; } = [];
}

/// <summary>
/// Alternative names, synonyms, and prior codes for this vaccine candidate.
/// Covers code names, historical names, partner organisation names, and registry identifiers.
/// </summary>
public class VaccinesCompanyCode
{
    public int Id { get; set; }
    public int VaccinesProductDetailId { get; set; }
    public string Name { get; set; } = null!;
    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesProductDetail VaccinesProductDetail { get; set; } = null!;
}

public class VaccinesCompanyInfo
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? IsOriginatorCompany { get; set; }

    /// <summary>Free text; conditional on IsOriginatorCompany = No.</summary>
    public string? OriginatorCompanyName { get; set; }

    public YesNoUnknown HasBeenAcquired { get; set; }

    /// <summary>Free text; conditional on HasBeenAcquired = Yes.</summary>
    public string? PreviousOwner { get; set; }

    public YesNoUnknown HasGrantFunding { get; set; }

    /// <summary>Free text; conditional on HasGrantFunding = Yes. Grant reference number or identifier.</summary>
    public string? GrantFundingIdentifier { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}

public class VaccinesDiseaseDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int? DiseaseAreaId { get; set; }

    /// <summary>
    /// Free text. The disease or diseases this vaccine prevents.
    /// e.g. Measles, mumps and rubella. Single field covers combination vaccines.
    /// </summary>
    public string DiseaseTarget { get; set; } = null!;

    /// <summary>
    /// Intended age group e.g. Infants under 12 months, Adults aged 65 and over,
    /// All ages, Unknown at this stage.
    /// </summary>
    public string AgeGroup { get; set; } = null!;

    /// <summary>
    /// Intended risk group if applicable e.g. Immunocompromised individuals,
    /// Pregnant women. Null if targeting general population.
    /// </summary>
    public string? RiskGroup { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.VaccineDiseaseArea? DiseaseArea { get; set; }
    public ICollection<VaccinesPathogen> Pathogens { get; set; } = [];
}

/// <summary>
/// One row per target pathogen. Handles both multivalent vaccines (e.g. MMR = 3 rows)
/// and polyvalent vaccines (multiple strains, one disease).
/// </summary>
public class VaccinesPathogen
{
    public int Id { get; set; }
    public int VaccinesDiseaseDetailId { get; set; }

    /// <summary>e.g. Measles virus, Streptococcus pneumoniae, RSV</summary>
    public string PathogenName { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesDiseaseDetail VaccinesDiseaseDetail { get; set; } = null!;
}

public class VaccinesTechnology
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int VaccinePlatformId { get; set; }

    /// <summary>Free text; conditional on VaccinePlatform = Other.</summary>
    public string? VaccinePlatformOther { get; set; }

    public int AdministrationRouteId { get; set; }
    public YesNoNotYetConfirmed HasAdjuvant { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.VaccinePlatform VaccinePlatform { get; set; } = null!;
    public ReferenceData.VaccineAdministrationRoute AdministrationRoute { get; set; } = null!;
    public ICollection<VaccinesAntigen> Antigens { get; set; } = [];
    public ICollection<VaccinesAdjuvant> Adjuvants { get; set; } = [];
}

/// <summary>
/// One row per antigen. The antigen is the specific biological component
/// the immune system learns to recognise — e.g. spike protein, polysaccharide capsule,
/// haemagglutinin. Replaces 'pathogen-related components' from the original JCVI proforma.
/// </summary>
public class VaccinesAntigen
{
    public int Id { get; set; }
    public int VaccinesTechnologyId { get; set; }
    public string AntigenName { get; set; } = null!;
    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesTechnology VaccinesTechnology { get; set; } = null!;
}

/// <summary>
/// One row per adjuvant component. Supports zero, one, or multiple adjuvants.
/// An adjuvant enhances the immune response but is not itself immunogenic —
/// distinct from the antigen. e.g. AS01, aluminium hydroxide, MF59.
/// </summary>
public class VaccinesAdjuvant
{
    public int Id { get; set; }
    public int VaccinesTechnologyId { get; set; }
    public string AdjuvantName { get; set; } = null!;
    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesTechnology VaccinesTechnology { get; set; } = null!;
}

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

public class VaccinesPopulation
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public string? AgeGroup { get; set; }
    public string? RiskGroup { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}
