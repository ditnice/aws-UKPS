using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

[FormSection(
    RecordType.Medicine,
    "patient-and-clinical-requirements",
    "Patient and clinical requirements",
    Order = 40
)]
internal sealed class MedicinesTreatmentDetail
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    /// <summary>Includes likely comparators.</summary>
    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage("place-in-therapy", "Proposed place in therapy", Order = 30)]
    [FormField(
        "medicines_treatment_detail.proposed_place_in_therapy",
        "Enter the proposed place in therapy and likely comparators",
        FormControl.Textarea,
        Hint = "Describe where this medicine is expected to sit in the treatment pathway, as well as which treatments it may replace or be compared against."
    )]
    [FormRequired("Enter the proposed place in therapy for your product")]
    public string? ProposedPlaceInTherapy { get; set; }

    /// <summary>CiC — Commercially in Confidence.</summary>
    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage(
        "treatment-duration",
        "Estimated duration of treatment",
        Order = 40,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_treatment_detail.estimated_duration_of_treatment",
        "Enter the estimated duration of treatment",
        FormControl.Textarea,
        Hint = "Enter the expected length of time a patient will receive this treatment. If treatment is not continuous, enter the mean duration.\n\nThis information is used to help calculate the cost of treatment per patient and to plan service capacity, for example, understanding whether a new treatment requires more or less clinical time than existing options."
    )]
    [FormRequired("Enter the estimated duration of treatment for your product")]
    public string? EstimatedDurationOfTreatment { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
