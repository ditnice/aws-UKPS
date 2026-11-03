using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

[FormSection(
    RecordType.Medicine,
    "patient-and-clinical-requirements",
    "Patient and clinical requirements",
    Order = 40
)]
internal sealed class MedicinesServiceImpact
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    /// <summary>
    /// The extent of NHS service change required to deliver this medicine.
    /// </summary>
    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage("nhs-service-changes", "NHS service changes", Order = 30)]
    [FormField(
        "medicines_service_impact.nhs_service_changes_required",
        "Are any changes to NHS services required for the delivery of this product?",
        FormControl.Radio
    )]
    [FormRequired("Select if any changes to NHS services are required to deliver this product")]
    public NhsServiceChangesRequired? NhsServiceChangesRequired { get; set; }

    /// <summary>
    /// Conditional on NhsServiceChangesRequired = CompleteTransformation or SomeChange.
    /// Describes changes to staffing, equipment, pathway, or commissioning.
    /// </summary>
    [FormIgnore("Conditional reveal not supported yet")]
    public string? NhsServiceChangesDetails { get; set; }

    /// <summary>
    /// Are there specific requirements for the handling or storage of this product
    /// that may cause service issues?
    /// </summary>
    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage("handling-storage", "Handling and storage", Order = 40)]
    [FormField(
        "medicines_service_impact.handling_storage_requirements",
        "Are there specific requirements for the handling or storage of this product that may cause service issues?",
        FormControl.Radio,
        Hint = "Select yes if the storage, handling, or preparation of this medicine introduces service readiness requirements. For example, this medicine requires aseptic facilities, trained staff, autoinjectors, fridge storage, etc."
    )]
    [FormRequired(
        "Select yes if there are specific requirements for handling or storing the product that may cause service issues"
    )]
    public YesNoUnknown? HandlingStorageRequirements { get; set; }

    /// <summary>Conditional on HandlingStorageRequirements = Yes.</summary>
    [FormIgnore("Conditional reveal not supported yet")]
    public string? HandlingStorageDetails { get; set; }

    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage(
        "population-range",
        "Enter the UK patient population range for the condition",
        Order = 50
    )]
    [FormField(
        "medicines_service_impact.uk_patient_population_range_id",
        "Select the range estimate for the overall UK patient population for this condition",
        FormControl.Select,
        Placeholder = "Choose population size",
        Order = 10
    )]
    [FormRequired("Select a population size")]
    public int? UkPatientPopulationRangeId { get; set; }

    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage(
        "population-range",
        "Enter the UK patient population range for the condition",
        Order = 50
    )]
    [FormField(
        "medicines_service_impact.uk_patient_population_notes",
        "UK patient population notes",
        FormControl.Textarea,
        Hint = "Use UK data where possible. If UK data is not available, use reliable data from England, Scotland, or Wales and extrapolate to estimate the UK population.\n\nExamples of acceptable sources include guidance from the National Institute for Health and Care Excellence (NICE), epidemiology data from NHS National Services Scotland (NHS NSS) or Health Protection Scotland (HPS), or Welsh health statistics.\n\nIf epidemiology data about this condition is not available from these sources, data that has been obtained from a review of published literature may be used instead. Always provide references for the sources used.\n\nIf complex calculations or assumptions are used, explain them clearly.\n\nProvide references and calculations",
        Order = 20
    )]
    [FormRequired("Enter UK patient population notes")]
    public string? UkPatientPopulationNotes { get; set; }

    /// <summary>
    /// Free text. Expected to contain a range along with supporting references and calculations.
    /// </summary>
    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage(
        "eligible-population",
        "Enter the estimated UK eligible population range",
        Order = 60
    )]
    [FormField(
        "medicines_service_impact.estimated_eligible_patient_population",
        "Estimated UK eligible population range",
        FormControl.Textarea,
        Hint = "Provide an estimate of patients likely to be eligible for treatment in the UK. Include references and outline the calculation approach.\n\nInclude any relevant references and calculations.",
        Order = 10
    )]
    [FormRequired("Enter an estimated UK eligible population range")]
    public string? EstimatedEligiblePatientPopulation { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.UkPatientPopulationRange? UkPatientPopulationRange { get; set; }
}
