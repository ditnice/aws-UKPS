using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// The sections, tasks and pages of the Medicine form. Each question names its page with one of
/// the page ID constants here.
/// </summary>
/// <remarks>
/// Section, task and page IDs, titles and orders are part of the form fingerprint, as is a page's
/// "Commercial in confidence" flag, so changing one needs a form version bump.
/// </remarks>
internal sealed class MedicineFormLayout()
    : FormLayout(
        RecordType.Medicine,
        [
            new FormLayoutSection(
                "product-record-details",
                "Provide product record details",
                0,
                [
                    new FormLayoutTask(
                        "names-and-identifiers",
                        "Names and identifiers",
                        10,
                        [
                            new FormLayoutPage(
                                NamesAndIdentifiers,
                                "Provide product record details",
                                10
                            ),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "indication-and-development",
                "Provide indication and development information",
                10,
                [
                    new FormLayoutTask(
                        "indication-details",
                        "Indication details",
                        10,
                        [
                            new FormLayoutPage(Indication, "Indication", 10),
                            new FormLayoutPage(BnfChapter, "BNF chapter", 20),
                            new FormLayoutPage(TherapeuticArea, "Therapeutic area", 30),
                            new FormLayoutPage(Paediatric, "Treating children", 40),
                            new FormLayoutPage(Cancer, "Treating cancer", 50),
                            new FormLayoutPage(RareDisease, "Treating a rare disease", 60),
                            new FormLayoutPage(Formulation, "Formulation", 70),
                            new FormLayoutPage(Presentation, "Presentation", 80),
                            new FormLayoutPage(ModeOfAction, "Mode of action", 90),
                            new FormLayoutPage(DoseRegimen, "Proposed dose regimen", 100),
                            new FormLayoutPage(PersonalisedMedicine, "Personalised medicine", 110),
                            new FormLayoutPage(TechnologyStatus, "Technology status", 120),
                        ]
                    ),
                    new FormLayoutTask(
                        "development-background",
                        "Development background",
                        20,
                        [
                            new FormLayoutPage(RepurposedMedicine, "Repurposed medicine", 10),
                            new FormLayoutPage(Originator, "Original developer", 20),
                            new FormLayoutPage(CoMarketing, "Co-marketing", 30),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "clinical-trial-information",
                "Provide clinical trial information",
                20,
                [
                    new FormLayoutTask(
                        "clinical-trials",
                        "Clinical trials",
                        10,
                        [
                            new FormLayoutPage(AddClinicalTrial, "Add a clinical trial", 10),
                            new FormLayoutPage(ClinicalTrialDetails, "Clinical trial details", 20),
                            new FormLayoutPage(
                                RecruitingInUk,
                                "Recruiting in the UK",
                                30,
                                CommercialInConfidence: true
                            ),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "regulatory-access-and-launch",
                "Provide regulatory, access, and launch information",
                30,
                [
                    new FormLayoutTask(
                        "mhra-procedure-and-dates",
                        "Medicines and Healthcare products Regulatory Agency (MHRA) procedure and dates",
                        10,
                        [
                            new FormLayoutPage(
                                UkMarketAuthorisation,
                                "UK market authorisation",
                                10,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                UkSubmissionDate,
                                "UK regulatory submission date",
                                20,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                GlobalFirstSubmission,
                                "Global first submission (GFS)",
                                30,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                UkLicenceDate,
                                "UK regulatory licence date",
                                40,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                UkConditionalApproval,
                                "UK conditional approval",
                                50,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                InternationalRecognition,
                                "International Recognition Procedure (IRP)",
                                60,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                InternationalSubmissionDate,
                                "International regulatory submission date",
                                70,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                InternationalLicenceDate,
                                "International regulatory licence date",
                                80,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                InternationalConditionalApproval,
                                "International conditional approval",
                                90
                            ),
                        ]
                    ),
                    new FormLayoutTask(
                        "hta-and-launch",
                        "Health Technology Assessment (HTA) and launch",
                        20,
                        [
                            new FormLayoutPage(HtaSubmission, "HTA submission", 10),
                            new FormLayoutPage(NiceAlignedPathway, "MHRA/NICE aligned pathway", 20),
                            new FormLayoutPage(NiceTaId, "NICE technology appraisal ID", 30),
                            new FormLayoutPage(
                                UkLaunchDate,
                                "UK commercial launch date",
                                40,
                                CommercialInConfidence: true
                            ),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "special-designation",
                "Provide special designation information",
                50,
                [
                    new FormLayoutTask(
                        "orphan-and-atmp",
                        "Orphan medicine status and Advanced Therapy Medicinal Product (ATMP) classification",
                        10,
                        [
                            new FormLayoutPage(OrphanStatus, "Orphan medicine status", 10),
                            new FormLayoutPage(AtmpClassification, "ATMP classification", 20),
                        ]
                    ),
                    new FormLayoutTask(
                        "pim-and-eams",
                        "Promising Innovative Medicine (PIM) and Early Access to Medicines Scheme (EAMS)",
                        20,
                        [
                            new FormLayoutPage(
                                PimDesignation,
                                "PIM designation",
                                10,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                EamsSubmissionDate,
                                "EAMS submission date",
                                20,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                EamsOpinionDate,
                                "EAMS scientific opinion date",
                                30,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                EamsOpinionDecision,
                                "EAMS scientific opinion decision",
                                40,
                                CommercialInConfidence: true
                            ),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "service-readiness",
                "Provide service readiness information (optional)",
                60,
                [
                    new FormLayoutTask(
                        "laboratory-testing",
                        "Laboratory testing",
                        10,
                        [
                            new FormLayoutPage(
                                DiagnosticTestRequired,
                                "Diagnostic test",
                                10,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                BiomarkerType,
                                "Biomarker type",
                                20,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                NonGenomicBiomarker,
                                "Non-genomic biomarker",
                                30,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                GenomicTarget,
                                "Genomic target",
                                40,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                NgtdRelationship,
                                "NGTD",
                                50,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                SampleType,
                                "Sample type",
                                60,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                TurnaroundTime,
                                "Turnaround time",
                                70,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                PathwayPoint,
                                "Patient pathway",
                                80,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                GenomicAlterations,
                                "Genomic alterations",
                                90,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                AdditionalGenomicFactors,
                                "Additional genomic factors",
                                100,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                TestUsedInTrials,
                                "Test used in trials",
                                110,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                SpecificitySensitivity,
                                "Specificity and sensitivity",
                                120,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                TestMandatory,
                                "Mandatory testing",
                                130,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                MonitoringTests,
                                "Monitoring tests",
                                140,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                SafetyTests,
                                "Safety tests",
                                150,
                                CommercialInConfidence: true
                            ),
                        ]
                    ),
                    new FormLayoutTask(
                        "patient-and-clinical-requirements",
                        "Patient and clinical requirements",
                        20,
                        [
                            new FormLayoutPage(
                                ScreeningProgramme,
                                "Screening or surveillance",
                                10,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                UrgentIdentification,
                                "Urgent patient identification",
                                20,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(PlaceInTherapy, "Proposed place in therapy", 30),
                            new FormLayoutPage(
                                TreatmentDuration,
                                "Estimated duration of treatment",
                                40,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(NhsServiceChanges, "NHS service changes", 50),
                            new FormLayoutPage(HandlingStorage, "Handling and storage", 60),
                            new FormLayoutPage(PopulationRange, "UK patient population", 70),
                            new FormLayoutPage(
                                EligiblePopulation,
                                "Enter the estimated UK eligible population range",
                                80
                            ),
                        ]
                    ),
                    new FormLayoutTask(
                        "pricing-and-budget-impact",
                        "Pricing and budget impact",
                        30,
                        [
                            new FormLayoutPage(
                                EstimatedUptake,
                                "Estimated uptake",
                                10,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                CompassionateAccess,
                                "Compassionate or early access",
                                20
                            ),
                            new FormLayoutPage(
                                PatientAccessScheme,
                                "Patient Access Scheme",
                                30,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                IndicationSpecificPricing,
                                "Indication specific pricing",
                                40,
                                CommercialInConfidence: true
                            ),
                            new FormLayoutPage(
                                NetBudgetImpact,
                                "Net budget impact",
                                50,
                                CommercialInConfidence: true
                            ),
                        ]
                    ),
                ]
            ),
        ]
    )
{
    /// <summary>The "Provide product record details" page's ID.</summary>
    public const string NamesAndIdentifiers = "names-and-identifiers";

    /// <summary>The "Indication" page's ID.</summary>
    public const string Indication = "indication";

    /// <summary>The "BNF chapter" page's ID.</summary>
    public const string BnfChapter = "bnf-chapter";

    /// <summary>The "Therapeutic area" page's ID.</summary>
    public const string TherapeuticArea = "therapeutic-area";

    /// <summary>The "Treating children" page's ID.</summary>
    public const string Paediatric = "paediatric";

    /// <summary>The "Treating cancer" page's ID.</summary>
    public const string Cancer = "cancer";

    /// <summary>The "Treating a rare disease" page's ID.</summary>
    public const string RareDisease = "rare-disease";

    /// <summary>The "Formulation" page's ID.</summary>
    public const string Formulation = "formulation";

    /// <summary>The "Presentation" page's ID.</summary>
    public const string Presentation = "presentation";

    /// <summary>The "Mode of action" page's ID.</summary>
    public const string ModeOfAction = "mode-of-action";

    /// <summary>The "Proposed dose regimen" page's ID.</summary>
    public const string DoseRegimen = "dose-regimen";

    /// <summary>The "Personalised medicine" page's ID.</summary>
    public const string PersonalisedMedicine = "personalised-medicine";

    /// <summary>The "Technology status" page's ID.</summary>
    public const string TechnologyStatus = "technology-status";

    /// <summary>The "Repurposed medicine" page's ID.</summary>
    public const string RepurposedMedicine = "repurposed-medicine";

    /// <summary>The "Original developer" page's ID.</summary>
    public const string Originator = "originator";

    /// <summary>The "Co-marketing" page's ID.</summary>
    public const string CoMarketing = "co-marketing";

    /// <summary>The "Add a clinical trial" page's ID.</summary>
    public const string AddClinicalTrial = "add-clinical-trial";

    /// <summary>The "Clinical trial details" page's ID.</summary>
    public const string ClinicalTrialDetails = "clinical-trial-details";

    /// <summary>The "Recruiting in the UK" page's ID.</summary>
    public const string RecruitingInUk = "recruiting-in-uk";

    /// <summary>The "UK market authorisation" page's ID.</summary>
    public const string UkMarketAuthorisation = "uk-market-authorisation";

    /// <summary>The "UK regulatory submission date" page's ID.</summary>
    public const string UkSubmissionDate = "uk-submission-date";

    /// <summary>The "Global first submission (GFS)" page's ID.</summary>
    public const string GlobalFirstSubmission = "global-first-submission";

    /// <summary>The "UK regulatory licence date" page's ID.</summary>
    public const string UkLicenceDate = "uk-licence-date";

    /// <summary>The "UK conditional approval" page's ID.</summary>
    public const string UkConditionalApproval = "uk-conditional-approval";

    /// <summary>The "International Recognition Procedure (IRP)" page's ID.</summary>
    public const string InternationalRecognition = "international-recognition";

    /// <summary>The "International regulatory submission date" page's ID.</summary>
    public const string InternationalSubmissionDate = "international-submission-date";

    /// <summary>The "International regulatory licence date" page's ID.</summary>
    public const string InternationalLicenceDate = "international-licence-date";

    /// <summary>The "International conditional approval" page's ID.</summary>
    public const string InternationalConditionalApproval = "international-conditional-approval";

    /// <summary>The "HTA submission" page's ID.</summary>
    public const string HtaSubmission = "hta-submission";

    /// <summary>The "MHRA/NICE aligned pathway" page's ID.</summary>
    public const string NiceAlignedPathway = "nice-aligned-pathway";

    /// <summary>The "NICE technology appraisal ID" page's ID.</summary>
    public const string NiceTaId = "nice-ta-id";

    /// <summary>The "UK commercial launch date" page's ID.</summary>
    public const string UkLaunchDate = "uk-launch-date";

    /// <summary>The "Orphan medicine status" page's ID.</summary>
    public const string OrphanStatus = "orphan-status";

    /// <summary>The "ATMP classification" page's ID.</summary>
    public const string AtmpClassification = "atmp-classification";

    /// <summary>The "PIM designation" page's ID.</summary>
    public const string PimDesignation = "pim-designation";

    /// <summary>The "EAMS submission date" page's ID.</summary>
    public const string EamsSubmissionDate = "eams-submission-date";

    /// <summary>The "EAMS scientific opinion date" page's ID.</summary>
    public const string EamsOpinionDate = "eams-opinion-date";

    /// <summary>The "EAMS scientific opinion decision" page's ID.</summary>
    public const string EamsOpinionDecision = "eams-opinion-decision";

    /// <summary>The "Diagnostic test" page's ID.</summary>
    public const string DiagnosticTestRequired = "diagnostic-test-required";

    /// <summary>The "Biomarker type" page's ID.</summary>
    public const string BiomarkerType = "biomarker-type";

    /// <summary>The "Non-genomic biomarker" page's ID.</summary>
    public const string NonGenomicBiomarker = "non-genomic-biomarker";

    /// <summary>The "Genomic target" page's ID.</summary>
    public const string GenomicTarget = "genomic-target";

    /// <summary>The "NGTD" page's ID.</summary>
    public const string NgtdRelationship = "ngtd-relationship";

    /// <summary>The "Sample type" page's ID.</summary>
    public const string SampleType = "sample-type";

    /// <summary>The "Turnaround time" page's ID.</summary>
    public const string TurnaroundTime = "turnaround-time";

    /// <summary>The "Patient pathway" page's ID.</summary>
    public const string PathwayPoint = "pathway-point";

    /// <summary>The "Genomic alterations" page's ID.</summary>
    public const string GenomicAlterations = "genomic-alterations";

    /// <summary>The "Additional genomic factors" page's ID.</summary>
    public const string AdditionalGenomicFactors = "additional-genomic-factors";

    /// <summary>The "Test used in trials" page's ID.</summary>
    public const string TestUsedInTrials = "test-used-in-trials";

    /// <summary>The "Specificity and sensitivity" page's ID.</summary>
    public const string SpecificitySensitivity = "specificity-sensitivity";

    /// <summary>The "Mandatory testing" page's ID.</summary>
    public const string TestMandatory = "test-mandatory";

    /// <summary>The "Monitoring tests" page's ID.</summary>
    public const string MonitoringTests = "monitoring-tests";

    /// <summary>The "Safety tests" page's ID.</summary>
    public const string SafetyTests = "safety-tests";

    /// <summary>The "Screening or surveillance" page's ID.</summary>
    public const string ScreeningProgramme = "screening-programme";

    /// <summary>The "Urgent patient identification" page's ID.</summary>
    public const string UrgentIdentification = "urgent-identification";

    /// <summary>The "Proposed place in therapy" page's ID.</summary>
    public const string PlaceInTherapy = "place-in-therapy";

    /// <summary>The "Estimated duration of treatment" page's ID.</summary>
    public const string TreatmentDuration = "treatment-duration";

    /// <summary>The "NHS service changes" page's ID.</summary>
    public const string NhsServiceChanges = "nhs-service-changes";

    /// <summary>The "Handling and storage" page's ID.</summary>
    public const string HandlingStorage = "handling-storage";

    /// <summary>The "UK patient population" page's ID.</summary>
    public const string PopulationRange = "population-range";

    /// <summary>The "Enter the estimated UK eligible population range" page's ID.</summary>
    public const string EligiblePopulation = "eligible-population";

    /// <summary>The "Estimated uptake" page's ID.</summary>
    public const string EstimatedUptake = "estimated-uptake";

    /// <summary>The "Compassionate or early access" page's ID.</summary>
    public const string CompassionateAccess = "compassionate-access";

    /// <summary>The "Patient Access Scheme" page's ID.</summary>
    public const string PatientAccessScheme = "patient-access-scheme";

    /// <summary>The "Indication specific pricing" page's ID.</summary>
    public const string IndicationSpecificPricing = "indication-specific-pricing";

    /// <summary>The "Net budget impact" page's ID.</summary>
    public const string NetBudgetImpact = "net-budget-impact";
}
