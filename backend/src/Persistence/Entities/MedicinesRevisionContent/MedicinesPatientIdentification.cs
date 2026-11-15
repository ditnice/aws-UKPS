using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

[FormSection(
    RecordType.Medicine,
    MedicineFormSections.ServiceReadiness,
    MedicineFormSections.ServiceReadinessTitle,
    Order = MedicineFormSections.ServiceReadinessOrder
)]
internal sealed class MedicinesPatientIdentification
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage(
        "screening-programme",
        "Screening or surveillance",
        Order = 10,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_patient_identification.screening_required",
        "Is a screening or surveillance programme required to identify eligible patients?",
        FormControl.Radio,
        Order = 10,
        Hint = "Select yes if identifying patients requires proactive screening, testing, or surveillance beyond usual care. Select no if patients are typically identified through existing clinical pathways. Select unknown if this is not yet clear."
    )]
    [FormRequired(
        "Select yes if a screening or surveillance programme is required to identify eligible patients"
    )]
    public YesNoUnknown? ScreeningRequired { get; set; }

    /// <summary>Conditional on ScreeningRequired = Yes.</summary>
    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage(
        "screening-programme",
        "Screening or surveillance",
        Order = 10,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_patient_identification.screening_details",
        "Briefly describe the type of screening or surveillance needed",
        FormControl.Textarea,
        Order = 20,
        Hint = "Include details of new processes or adaptations to existing systems that may be needed."
    )]
    [FormShowWhen("medicines_patient_identification.screening_required", YesNoUnknown.Yes)]
    [FormRequired("Enter details")]
    public string? ScreeningDetails { get; set; }

    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage(
        "urgent-identification",
        "Urgent patient identification",
        Order = 20,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_patient_identification.urgent_identification_required",
        "Do patients need to be identified for treatment more quickly than standard practice?",
        FormControl.Radio,
        Order = 10,
        Hint = "Select yes if standard timelines for identification or treatment would create adverse patient outcomes. Select no if standard timelines are acceptable. Select unknown if this has not yet been determined."
    )]
    [FormRequired(
        "Select yes if urgent patient identification and treatment delivery is required for your product"
    )]
    public YesNoUnknown? UrgentIdentificationRequired { get; set; }

    /// <summary>Conditional on UrgentIdentificationRequired = Yes.</summary>
    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage(
        "urgent-identification",
        "Urgent patient identification",
        Order = 20,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_patient_identification.urgent_identification_details",
        "Provide further details",
        FormControl.Textarea,
        Order = 20,
        Hint = "Explain why faster identification or treatment may be required and what this could mean for services, at a high level. Avoid detailed operational planning."
    )]
    [FormShowWhen(
        "medicines_patient_identification.urgent_identification_required",
        YesNoUnknown.Yes
    )]
    [FormRequired("Enter details")]
    public string? UrgentIdentificationDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
