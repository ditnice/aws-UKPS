using UKPS.Data.Enums;

namespace UKPS.Data.Entities.MedicinesRevisionContent;

public class MedicinesProductDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>
    /// Short human-readable label to identify this record on the homepage.
    /// e.g. Chronic hepatitis C in adults.
    /// </summary>
    public string RecordTitle { get; set; } = null!;

    public string? BrandedName { get; set; }
    public string Indication { get; set; } = null!;
    public IndicationPaediatricStatus? IndicationIsPaediatric { get; set; }
    public YesNoUnknown? IndicationIsCancer { get; set; }
    public int? BnfChapterId { get; set; }
    public int? TherapeuticAreaId { get; set; }
    public int? FormulationTypeId { get; set; }
    public string? Presentation { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.BnfChapter? BnfChapter { get; set; }
    public ReferenceData.TherapeuticArea? TherapeuticArea { get; set; }
    public ReferenceData.FormulationType? FormulationType { get; set; }
    public ICollection<MedicinesActiveSubstance> ActiveSubstances { get; set; } = [];
    public ICollection<MedicinesRecordStatus> RecordStatuses { get; set; } = [];
}

public class MedicinesActiveSubstance
{
    public int Id { get; set; }
    public int MedicinesProductDetailId { get; set; }
    public string Name { get; set; } = null!;
    public SubstanceNameType NameType { get; set; }
    public int? DisplayOrder { get; set; }

    // Navigation
    public MedicinesProductDetail MedicinesProductDetail { get; set; } = null!;
}

/// <summary>Junction table: technology status types selected for a medicine record.</summary>
public class MedicinesRecordStatus
{
    public int MedicinesProductDetailId { get; set; }
    public int MedicineStatusTypeId { get; set; }

    // Navigation
    public MedicinesProductDetail MedicinesProductDetail { get; set; } = null!;
    public ReferenceData.MedicineTechnologyStatus MedicineStatusType { get; set; } = null!;
}

public class MedicinesCompanyInfo
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? IsOriginatorCompany { get; set; }

    /// <summary>Free text; conditional on IsOriginatorCompany = No.</summary>
    public string? OriginatorCompanyName { get; set; }

    public YesNoUnknown? IsCoMarketed { get; set; }

    /// <summary>Free text; conditional on IsCoMarketed = Yes.</summary>
    public string? CoMarketingCompanyName { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}

public class MedicinesDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public string? ModeOfAction { get; set; }
    public string? ProposedDoseRegimen { get; set; }
    public YesNoUnknown? IsPersonalisedMedicine { get; set; }
    public YesNoUnknown? IsRepurposedMedicine { get; set; }

    /// <summary>Conditional on IsRepurposedMedicine = Yes.</summary>
    public string? RepurposedMedicineDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}

/// <summary>Owns EamsSubmission and EamsOpinion regulatory date rows.</summary>
public class MedicinesEamsPim
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public PimDesignationStatus? PimDesignationStatus { get; set; }

    /// <summary>Conditional on PimDesignationStatus = Granted.</summary>
    public YesNoUnknown? WillSubmitToEams { get; set; }

    public EamsOpinionDecision? EamsOpinionDecision { get; set; }
    public int? EamsSubmissionDateId { get; set; }
    public int? EamsOpinionDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public SharedRevisionContent.RegulatoryDate? EamsSubmissionDate { get; set; }
    public SharedRevisionContent.RegulatoryDate? EamsOpinionDate { get; set; }
}

/// <summary>Owns the EuOrphanGranted regulatory date row.</summary>
public class MedicinesEuStatus
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public OrphanAtmpStatus? EuOrphanStatus { get; set; }
    public string? EuOrphanStatusNumber { get; set; }
    public int? EuOrphanGrantedDateId { get; set; }
    public OrphanAtmpStatus? EuAtmpClassificationStatus { get; set; }
    public int? AtmpClassificationId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public SharedRevisionContent.RegulatoryDate? EuOrphanGrantedDate { get; set; }
    public ReferenceData.AtmpClassification? AtmpClassification { get; set; }
}

public class MedicinesPatientIdentification
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? ScreeningRequired { get; set; }

    /// <summary>Conditional on ScreeningRequired = Yes.</summary>
    public string? ScreeningDetails { get; set; }

    public YesNoUnknown? UrgentIdentificationRequired { get; set; }

    /// <summary>Conditional on UrgentIdentificationRequired = Yes.</summary>
    public string? UrgentIdentificationDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}

public class MedicinesLaboratoryTesting
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? DiagnosticTestRequired { get; set; }
    public YesNoUnknown? GenomicTestRequired { get; set; }
    public YesNoUnknown? GenomicTestInNationalDirectory { get; set; }
    public string? NationalGenomicTestDirectoryId { get; set; }
    public int? GenomicSampleTypeId { get; set; }
    public string? GenomicSampleTypeOther { get; set; }
    public YesNoUnknown? GenomicTurnaroundConsiderations { get; set; }
    public int? PatientPathwayPointId { get; set; }
    public string? GenomicTestPathwayPointOther { get; set; }
    public string? GenomicBiomarker { get; set; }
    public string? GenomicAlterations { get; set; }
    public string? GenomicTestUsedInTrials { get; set; }
    public string? GenomicTestSpecificitySensitivity { get; set; }
    public string? GenomicCoMutations { get; set; }
    public YesNoUnknown? GenomicTestMandatory { get; set; }
    public string? GenomicTestNotes { get; set; }
    public YesNoUnknown? MonitoringTestsRequired { get; set; }

    /// <summary>Conditional on MonitoringTestsRequired = Yes.</summary>
    public string? MonitoringTestsDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.GenomicSampleType? GenomicSampleType { get; set; }
    public ReferenceData.PatientPathwayPoint? PatientPathwayPoint { get; set; }
}

public class MedicinesTreatmentDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>Includes likely comparators.</summary>
    public string ProposedPlaceInTherapy { get; set; } = null!;

    /// <summary>CiC — Commercially in Confidence.</summary>
    public string? EstimatedDurationOfTreatment { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}

public class MedicinesServiceImpact
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? ExistingNhsService { get; set; }
    public string? NhsServiceRedesignDetails { get; set; }
    public int? UkPatientPopulationRangeId { get; set; }
    public string? UkPatientPopulationNotes { get; set; }
    public string? EstimatedEligiblePatientPopulation { get; set; }
    public YesNoUnknown? CompassionateAccessAvailable { get; set; }

    /// <summary>Conditional on CompassionateAccessAvailable = Yes.</summary>
    public string? CompassionateAccessDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.UkPatientPopulationRange? UkPatientPopulationRange { get; set; }
}

public class MedicinesBudgetImpact
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>CiC — Commercially in Confidence.</summary>
    public YesNoUnknown? IndicationSpecificPricingPlanned { get; set; }

    /// <summary>Conditional on IndicationSpecificPricingPlanned = Yes.</summary>
    public string? IndicationSpecificPricingDetails { get; set; }

    /// <summary>CiC — Commercially in Confidence.</summary>
    public YesNoUnknown? NetUkBudgetImpactOver5M { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ICollection<MedicinesPasRegion> PasRegions { get; set; } = [];
}

/// <summary>Junction table: PAS regions selected for a medicine record.</summary>
public class MedicinesPasRegion
{
    public int MedicinesBudgetImpactId { get; set; }
    public int PasRegionId { get; set; }

    // Navigation
    public MedicinesBudgetImpact MedicinesBudgetImpact { get; set; } = null!;
    public ReferenceData.PasRegion PasRegion { get; set; } = null!;
}
