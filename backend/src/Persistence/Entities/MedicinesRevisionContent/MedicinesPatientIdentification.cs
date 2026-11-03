using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

[FormSection(
    RecordType.Medicine,
    "patient-and-clinical-requirements",
    "Patient and clinical requirements",
    Order = 40
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
    [FormPage("screening-programme", "Screening or surveillance", Order = 20)]
    [FormField(
        "medicines_patient_identification.screening_required",
        "Is a screening or surveillance programme required to identify eligible patients?",
        FormControl.Radio,
        Hint = "Select yes if identifying patients requires proactive screening, testing, or surveillance beyond usual care. Select no if patients are typically identified through existing clinical pathways. Select unknown if this is not yet clear."
    )]
    [FormRequired(
        "Select yes if a screening or surveillance programme is required to identify eligible patients"
    )]
    public YesNoUnknown? ScreeningRequired { get; set; }

    /// <summary>Conditional on ScreeningRequired = Yes.</summary>
    [FormIgnore("Conditional reveal not supported yet")]
    public string? ScreeningDetails { get; set; }

    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage("urgent-identification", "Urgent patient identification", Order = 10)]
    [FormField(
        "medicines_patient_identification.urgent_identification_required",
        "Do patients need to be identified for treatment more quickly than standard practice?",
        FormControl.Radio,
        Hint = "Select yes if standard timelines for identification or treatment would create adverse patient outcomes. Select no if standard timelines are acceptable. Select unknown if this has not yet been determined."
    )]
    [FormRequired(
        "Select yes if urgent patient identification and treatment delivery is required for your product"
    )]
    public YesNoUnknown? UrgentIdentificationRequired { get; set; }

    /// <summary>Conditional on UrgentIdentificationRequired = Yes.</summary>
    [FormIgnore("Conditional reveal not supported yet")]
    public string? UrgentIdentificationDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
