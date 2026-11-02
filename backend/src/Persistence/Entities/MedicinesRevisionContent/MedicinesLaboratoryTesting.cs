using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

[FormSection(RecordType.Medicine, "laboratory-testing", "Laboratory testing", Order = 30)]
internal sealed class MedicinesLaboratoryTesting
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("diagnostic-test-required", "Diagnostic test", Order = 10)]
    [FormField(
        "medicines_laboratory_testing.diagnostic_test_required",
        "Does the product require a specific biomarker or diagnostic test result before prescribing?",
        FormControl.Radio,
        Hint = "Select yes if patient eligibility for this treatment, or the dose or regimen used, depends on the result of a diagnostic or biomarker test. This includes genomic tests, for example, mutation analysis, sequencing. It can also include non-genomic tests, for example, protein expression, hormone receptor status, cell counts."
    )]
    [FormRequired(
        "Select yes if the product requires a specific biomarker or test before prescribing"
    )]
    public YesNoUnknown? DiagnosticTestRequired { get; set; }

    /// <summary>
    /// Determines the biomarker type. Conditional on DiagnosticTestRequired = Yes.
    /// </summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("biomarker-type", "Biomarker type", Order = 20)]
    [FormField(
        "medicines_laboratory_testing.biomarker_type",
        "What type of biomarker is required for this test?",
        FormControl.Radio,
        Hint = "Select the type of biomarker that determines whether a patient is eligible for this treatment, or affects how it is given."
    )]
    [FormRequired("Select the biomarker required for this test")]
    public BiomarkerType? BiomarkerType { get; set; }

    /// <summary>
    /// Conditional on BiomarkerType = NonGenomicBiomarker.
    /// Describes the non-genomic biomarker and how it is measured.
    /// </summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("non-genomic-biomarker", "Non-genomic biomarker", Order = 30)]
    [FormField(
        "medicines_laboratory_testing.non_genomic_biomarker_description",
        "Describe the non-genomic biomarker",
        FormControl.Textarea,
        Hint = "Describe the biomarker and how it is measured. Include the threshold or cut-off value used to determine eligibility where known."
    )]
    [FormRequired("Enter details of the non-genomic biomarker")]
    public string? NonGenomicBiomarkerDescription { get; set; }

    /// <summary>
    /// The specific gene or genomic target the test detects.
    /// Conditional on BiomarkerType = GenomicBiomarker.
    /// </summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("genomic-target", "Genomic target", Order = 40)]
    [FormField(
        "medicines_laboratory_testing.genomic_target",
        "What is the genomic target for this test?",
        FormControl.Text,
        Hint = "Specify the gene or genomic target the test detects. For example, enter the specific mutation or biomarker."
    )]
    [FormRequired("Enter the genomic target for this test")]
    public string? GenomicTarget { get; set; }

    /// <summary>
    /// How the test relates to what is currently in the National Genomic Test Directory.
    /// Conditional on BiomarkerType = GenomicBiomarker.
    /// </summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("ngtd-relationship", "NGTD", Order = 50)]
    [FormField(
        "medicines_laboratory_testing.genomic_test_ngtd_relationship",
        "How does this test relate to what's currently in the National Genomic Test Directory (NGTD)?",
        FormControl.Radio,
        Hint = "Select the option that describes the current testing status of the genomic target identified in the previous question."
    )]
    [FormRequired(
        "Select how this test relates to what's currently in the National Genomic Test Directory"
    )]
    public GenomicTestNgtdRelationship? GenomicTestNgtdRelationship { get; set; }

    /// <summary>Free text. Conditional on BiomarkerType = GenomicBiomarker.</summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("sample-type", "Sample type", Order = 60)]
    [FormField(
        "medicines_laboratory_testing.genomic_sample_type",
        "What sample type is needed?",
        FormControl.Textarea,
        Hint = "Specify the sample type required for the test. For example, tissue, liquid, any other sample. If the same test may sometimes be run on different sample types, please describe the primary sample type and note any alternatives if relevant."
    )]
    [FormRequired("Enter details of the sample type required for the test")]
    public string? GenomicSampleType { get; set; }

    /// <summary>
    /// Describes turnaround time requirements differing from the current commissioned pathway.
    /// Optional. Conditional on BiomarkerType = GenomicBiomarker.
    /// </summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("turnaround-time", "Turnaround time", Order = 70)]
    [FormField(
        "medicines_laboratory_testing.genomic_turnaround_time_details",
        "Describe any turnaround time requirements for this test that differ from the current routinely commissioned testing pathway (optional)",
        FormControl.Textarea,
        Hint = "Describe the turnaround time needed for the full testing pathway, from sample collection to result, if this differs from what's currently offered. State the point you're measuring from, for example, sample collection or receipt at the lab. If you're not sure what the current turnaround time is, contact the Genomics Unit."
    )]
    public string? GenomicTurnaroundTimeDetails { get; set; }

    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("pathway-point", "Patient pathway", Order = 80)]
    [FormField(
        "medicines_laboratory_testing.patient_pathway_point_id",
        "At which point of the patient pathway should the test be conducted?",
        FormControl.Radio,
        Hint = "Select when in the care pathway the test is expected to take place."
    )]
    [FormRequired("Select the point in the patient pathway the test should be conducted")]
    public int? PatientPathwayPointId { get; set; }

    [FormIgnore("Conditional reveal not supported yet")]
    public string? GenomicTestPathwayPointOther { get; set; }

    /// <summary>What genomic alterations determine patient eligibility?</summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("genomic-alterations", "Genomic alterations", Order = 90)]
    [FormField(
        "medicines_laboratory_testing.genomic_alterations",
        "What genomic alterations determine patient eligibility?",
        FormControl.Textarea,
        Hint = "List the genomic alterations that determine whether a patient is eligible for this treatment. This includes alterations anticipated to be included in the marketing authorisation or HTA recommendation.\n\nInclude alterations that must be present for a patient to qualify, and alterations that would exclude a patient.\n\nWhere known, specify the gene and variant, the type of alteration, and whether it's somatic or germline. Types of alteration include SNV, structural variant, CNV, methylation profile and repeat expansion."
    )]
    [FormRequired("Enter the genomic alterations that determine patient eligibility")]
    public string? GenomicAlterations { get; set; }

    /// <summary>
    /// Additional genomic factors that affect treatment selection or sequencing
    /// beyond the primary eligibility alteration.
    /// Conditional on BiomarkerType = GenomicBiomarker.
    /// </summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("additional-genomic-factors", "Additional genomic factors", Order = 100)]
    [FormField(
        "medicines_laboratory_testing.additional_genomic_factors",
        "Are there additional genomic factors that affect treatment selection or sequencing?",
        FormControl.Textarea,
        Hint = "List any genomic factors beyond the primary eligibility alteration that affect treatment selection, dosing or sequencing.\n\nFor example, this could include co-mutations that mean a different dose is needed, resistance variants that would make the treatment ineffective, or cases where this test is part of a sequence of tests."
    )]
    [FormRequired("Enter details of genomic factors that affect treatment selection or sequencing")]
    public string? AdditionalGenomicFactors { get; set; }

    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("test-used-in-trials", "Test used in trials", Order = 110)]
    [FormField(
        "medicines_laboratory_testing.genomic_test_used_in_trials",
        "What test was used in trials?",
        FormControl.Textarea,
        Hint = "Describe the test used to determine eligibility in the clinical trials for this medicine. Say whether the test is approved by the FDA or an equivalent regulatory body, if known."
    )]
    [FormRequired("Enter details of the test used in trials")]
    public string? GenomicTestUsedInTrials { get; set; }

    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("specificity-sensitivity", "Specificity and sensitivity", Order = 120)]
    [FormField(
        "medicines_laboratory_testing.genomic_test_specificity_sensitivity",
        "Enter the minimum specificity and sensitivity used",
        FormControl.Textarea,
        Hint = "If you used a commercial test, include a link to publicly available data. If the test is not publicly available, provide an alternative source. For example, provide details from the test's laboratory manual."
    )]
    [FormRequired("Enter details of the minimum specificity and sensitivity used in testing")]
    public string? GenomicTestSpecificitySensitivity { get; set; }

    /// <summary>Conditional on DiagnosticTestRequired = Yes.</summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("test-mandatory", "Mandatory testing", Order = 130)]
    [FormField(
        "medicines_laboratory_testing.genomic_test_mandatory_status",
        "Is this test mandatory before treatment can start?",
        FormControl.Radio
    )]
    [FormRequired("Select if the test is mandatory before treatment can start")]
    public GenomicTestMandatoryStatus? GenomicTestMandatoryStatus { get; set; }

    [FormIgnore("Conditional reveal not supported yet")]
    public string? GenomicTestNotes { get; set; }

    /// <summary>
    /// Tests needed to monitor response to treatment beyond current NHS practice.
    /// Optional. Not conditional — shown regardless of biomarker type. CiC.
    /// </summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("monitoring-tests", "Monitoring tests", Order = 140)]
    [FormField(
        "medicines_laboratory_testing.monitoring_tests_details",
        "Describe any tests (genomic tests or otherwise) needed to monitor response to treatment beyond what is currently offered in NHS practice",
        FormControl.Textarea,
        Hint = "Describe any tests required to monitor how well this treatment is working that go beyond what is currently used in routine clinical practice. Include the type of test, how frequently it would need to be performed, and at what stage of treatment."
    )]
    [FormRequired(
        "Enter details of tests needed to monitor a response to treatment beyond what is offered by the NHS"
    )]
    public string? MonitoringTestsDetails { get; set; }

    /// <summary>
    /// Tests needed to assess safety of treatment beyond current NHS practice.
    /// Optional. Not conditional — shown regardless of biomarker type. CiC.
    /// </summary>
    [FormTask(
        MedicineFormTasks.LaboratoryTesting,
        MedicineFormTasks.LaboratoryTestingTitle,
        Order = MedicineFormTasks.LaboratoryTestingOrder
    )]
    [FormPage("safety-tests", "Safety tests", Order = 150)]
    [FormField(
        "medicines_laboratory_testing.safety_tests_details",
        "Describe any tests needed to assess safety of the treatment that are additional to what is currently used in routine clinical practice",
        FormControl.Textarea,
        Hint = "Describe any tests required to monitor for treatment-related side effects or toxicity that go beyond what is currently used in routine clinical practice. Include the type of test, frequency, and the duration of monitoring required."
    )]
    [FormRequired("Enter details of tests needed to assess the safety of the treatment")]
    public string? SafetyTestsDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.PatientPathwayPoint? PatientPathwayPoint { get; set; }
}
