using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

internal sealed record SyntheticRecord
{
    public required string SourceId { get; init; }
    public required string OrganisationName { get; init; }
    public required RecordType RecordType { get; init; }
    public required RecordStatus RecordStatus { get; init; }
    public required DateOnly CreatedAt { get; init; }
    public required DateOnly LastUpdatedAt { get; init; }
    public DateOnly? ReviewedAt { get; init; }
    public SyntheticStatusChange? StatusChange { get; init; }
    public required SyntheticProductRecordDetails ProductRecordDetails { get; init; }
    public required SyntheticIndicationAndDevelopment IndicationAndDevelopmentInformation { get; init; }
    public required SyntheticClinicalTrialInformation ClinicalTrialInformation { get; init; }
    public required SyntheticRegulatoryAccessAndLaunch RegulatoryAccessAndLaunchInformation { get; init; }
    public required SyntheticServiceReadiness ServiceReadinessInformation { get; init; }

    internal sealed record SyntheticStatusChange
    {
        public required RecordStatusChangeReason Reason { get; init; }
        public string? Note { get; init; }
        public required DateOnly ChangedAt { get; init; }
    }

    internal sealed record SyntheticRegulatoryDate
    {
        public required DateOnly DateValue { get; init; }
        public required DatePrecision DatePrecision { get; init; }
        public required bool IsConfidential { get; init; }
    }

    internal sealed record SyntheticCodedLabel
    {
        public required string Code { get; init; }
        public required string Label { get; init; }
    }

    internal sealed record SyntheticProductRecordDetails
    {
        public required SyntheticNamesAndIdentifiers NamesAndIdentifiers { get; init; }
    }

    internal sealed record SyntheticNamesAndIdentifiers
    {
        public required string CompanyCode { get; init; }
        public string? BrandedName { get; init; }
        public required IReadOnlyList<string> GenericNames { get; init; }
        public required IReadOnlyList<string> OtherIdentifiers { get; init; }
        public required string RecordTitle { get; init; }
    }

    internal sealed record SyntheticIndicationAndDevelopment
    {
        public required SyntheticIndicationDetails IndicationDetails { get; init; }
        public required SyntheticDevelopmentBackground DevelopmentBackground { get; init; }
    }

    internal sealed record SyntheticIndicationDetails
    {
        public string? Indication { get; init; }
        public SyntheticCodedLabel? BnfChapter { get; init; }
        public required IReadOnlyList<SyntheticCodedLabel> TherapeuticAreas { get; init; }
        public IndicationPaediatricStatus? IndicationIsPaediatric { get; init; }
        public YesNoUnknown? IndicationIsCancer { get; init; }
        public YesNoUnknown? IndicationIsRareDisease { get; init; }
        public string? FormulationType { get; init; }
        public string? Presentation { get; init; }
        public string? ModeOfAction { get; init; }
        public string? ProposedDoseRegimen { get; init; }
        public YesNoUnknown? IsPersonalisedMedicine { get; init; }
        public required IReadOnlyList<MedicineTechnologyStatus> MedicineTechnologyStatus { get; init; }
    }

    internal sealed record SyntheticDevelopmentBackground
    {
        public YesNoUnknown? IsRepurposedMedicine { get; init; }
        public string? RepurposedMedicineDetails { get; init; }
        public YesNoUnknown? IsOriginatorCompany { get; init; }
        public string? OriginatorCompanyName { get; init; }
        public YesNoUnknown? IsCoMarketed { get; init; }
        public string? CoMarketingCompanyName { get; init; }
    }

    internal sealed record SyntheticClinicalTrialInformation
    {
        public YesNoUnknown? RecruitingInUk { get; init; }
        public required IReadOnlyList<SyntheticClinicalTrial> ClinicalTrials { get; init; }
    }

    internal sealed record SyntheticClinicalTrial
    {
        public required string StudyName { get; init; }
        public required string ClinicalTrialsGovNumber { get; init; }
        public required IReadOnlyList<string> OtherClinicalTrialNumbers { get; init; }
        public TrialPhase? TrialPhase { get; init; }
        public string? BriefDescription { get; init; }
    }

    internal sealed record SyntheticRegulatoryAccessAndLaunch
    {
        public required SyntheticMhraProcedureAndDates MhraProcedureAndDates { get; init; }
        public required SyntheticHtaAndLaunch HealthTechnologyAssessmentAndLaunch { get; init; }
        public required SyntheticSpecialDesignations SpecialDesignations { get; init; }
    }

    internal sealed record SyntheticMhraProcedureAndDates
    {
        public string? MhraProcedureType { get; init; }
        public string? ProcedureDetails { get; init; }
        public SyntheticRegulatoryDate? UkSubmissionDate { get; init; }
        public string? GlobalFirstSubmissionRegion { get; init; }
        public SyntheticRegulatoryDate? GlobalSubmissionActualDate { get; init; }
        public SyntheticRegulatoryDate? UkLicenceDate { get; init; }
        public YesNoUnknown? UkConditionalApprovalAnticipated { get; init; }
        public string? IrpReferenceRegulator { get; init; }
        public string? IrpRoute { get; init; }
        public SyntheticRegulatoryDate? IntlSubmissionDate { get; init; }
        public SyntheticRegulatoryDate? IntlLicenceDate { get; init; }
        public YesNoUnknown? IntlConditionalApprovalAnticipated { get; init; }
    }

    internal sealed record SyntheticHtaAndLaunch
    {
        public YesNoUnknown? MedicineHtaSubmissionIntended { get; init; }
        public IReadOnlyList<MedicineHtaAssessor>? MedicineHtaBodies { get; init; }
        public string? HtaAdditionalDetails { get; init; }
        public YesNoUnknown? HtaNiceAlignedPathway { get; init; }
        public string? NiceTaDevelopmentId { get; init; }
        public SyntheticRegulatoryDate? UkLaunchDate { get; init; }
    }

    internal sealed record SyntheticSpecialDesignations
    {
        public DesignationStatus? EuOrphanStatus { get; init; }
        public SyntheticRegulatoryDate? EuOrphanGrantedDate { get; init; }
        public string? EuOrphanStatusNumber { get; init; }
        public DesignationStatus? EuAtmpClassificationStatus { get; init; }
        public SyntheticRegulatoryDate? AtmpRecommendationDate { get; init; }
        public string? AtmpClassification { get; init; }
        public DesignationStatus? PimDesignationStatus { get; init; }
        public YesNoUnknown? WillSubmitToEams { get; init; }
        public SyntheticRegulatoryDate? EamsSubmissionDate { get; init; }
        public SyntheticRegulatoryDate? EamsOpinionDate { get; init; }
        public EamsOpinionDecision? EamsOpinionDecision { get; init; }
    }

    internal sealed record SyntheticServiceReadiness
    {
        public required SyntheticLaboratoryTesting LaboratoryTestingDetails { get; init; }
        public required SyntheticPatientAndClinicalRequirements PatientAndClinicalRequirements { get; init; }
        public required SyntheticPricingAndBudgetImpact PricingAndBudgetImpact { get; init; }
    }

    internal sealed record SyntheticLaboratoryTesting
    {
        public YesNoUnknown? DiagnosticTestRequired { get; init; }
        public BiomarkerType? BiomarkerType { get; init; }
        public string? NonGenomicBiomarkerDescription { get; init; }
        public string? GenomicTarget { get; init; }
        public GenomicTestNgtdRelationship? GenomicTestNgtdRelationship { get; init; }
        public string? GenomicSampleType { get; init; }
        public string? GenomicTurnaroundTimeDetails { get; init; }
        public string? PatientPathwayPoint { get; init; }
        public string? GenomicTestPathwayPointOther { get; init; }
        public string? GenomicAlterations { get; init; }
        public string? AdditionalGenomicFactors { get; init; }
        public string? GenomicTestUsedInTrials { get; init; }
        public string? GenomicTestSpecificitySensitivity { get; init; }
        public GenomicTestMandatoryStatus? GenomicTestMandatoryStatus { get; init; }
        public string? GenomicTestNotes { get; init; }
        public string? MonitoringTestsDetails { get; init; }
        public string? SafetyTestsDetails { get; init; }
    }

    internal sealed record SyntheticPatientAndClinicalRequirements
    {
        public YesNoUnknown? ScreeningRequired { get; init; }
        public string? ScreeningDetails { get; init; }
        public YesNoUnknown? UrgentIdentificationRequired { get; init; }
        public string? UrgentIdentificationDetails { get; init; }
        public string? ProposedPlaceInTherapy { get; init; }
        public string? EstimatedDurationOfTreatment { get; init; }
        public NhsServiceChangesRequired? NhsServiceChangesRequired { get; init; }
        public string? NhsServiceChangesDetails { get; init; }
        public YesNoUnknown? HandlingStorageRequirements { get; init; }
        public string? HandlingStorageDetails { get; init; }
        public string? UkPatientPopulationRange { get; init; }
        public string? UkPatientPopulationNotes { get; init; }
        public string? EstimatedEligiblePatientPopulation { get; init; }
    }

    internal sealed record SyntheticPricingAndBudgetImpact
    {
        public string? EstimatedUptake { get; init; }
        public YesNoUnknown? CompassionateAccessAvailable { get; init; }
        public string? CompassionateAccessDetails { get; init; }
        public YesNoUnknown? PatientAccessSchemePlanned { get; init; }
        public IReadOnlyList<PatientAccessSchemeRegion>? PatientAccessSchemeRegions { get; init; }
        public YesNoUnknown? IndicationSpecificPricingPlanned { get; init; }
        public string? IndicationSpecificPricingDetails { get; init; }
        public NetUkBudgetImpactBand? NetUkBudgetImpactBand { get; init; }
    }
}
