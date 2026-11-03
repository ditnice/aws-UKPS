using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>Pricing and budget impact.</summary>
[FormSection(
    RecordType.Medicine,
    "patient-and-clinical-requirements",
    "Patient and clinical requirements",
    Order = 40
)]
internal sealed class MedicinesBudgetImpact
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    /// <summary>Estimated uptake based on expected adoption patterns. CiC.</summary>
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
        "medicines_budget_impact.estimated_uptake",
        "Estimated uptake based on expected adoption patterns",
        FormControl.Textarea,
        Hint = "Give your best estimate of how many patients are likely to receive this treatment in the first few years after launch, and any factors that might affect uptake, for example, whether it replaces an existing treatment or targets a new patient group.\n\nAny information provided is a crucial help for horizon scanners to assess the likely financial and service impact ahead of HTA review.",
        Order = 20
    )]
    [FormRequired("Enter the estimated uptake of this product based on expected adoption patterns")]
    public string? EstimatedUptake { get; set; }

    /// <summary>
    /// Will UK patients be able to access this product on a compassionate or early access
    /// basis, outside of clinical trials?
    /// </summary>
    [FormTask(
        MedicineFormTasks.PatientAndClinicalRequirements,
        MedicineFormTasks.PatientAndClinicalRequirementsTitle,
        Order = MedicineFormTasks.PatientAndClinicalRequirementsOrder
    )]
    [FormPage("early-access", "Early access", Order = 70)]
    [FormField(
        "medicines_budget_impact.compassionate_access_available",
        "Will UK patients be able to access this product on compassionate or early access basis, outside of clinical trials?",
        FormControl.Radio
    )]
    [FormRequired(
        "Select if UK patients will be able to access this product on compassionate or early access basis outside of clinical trials"
    )]
    public YesNoUnknown? CompassionateAccessAvailable { get; set; }

    /// <summary>Conditional on CompassionateAccessAvailable = Yes.</summary>
    [FormIgnore("Conditional reveal not supported yet")]
    public string? CompassionateAccessDetails { get; set; }

    /// <summary>
    /// Is a Patient Access Scheme or alternative discount arrangement planned
    /// for this indication? Gates the PatientAccessSchemeRegions selection. CiC.
    /// </summary>
    [FormIgnore("No page designed yet")]
    public YesNoUnknown? PatientAccessSchemePlanned { get; set; }

    /// <summary>Multi-select. Conditional on PatientAccessSchemePlanned = Yes.</summary>
    [FormIgnore("No page designed yet")]
    public PatientAccessSchemeRegion? PatientAccessSchemeRegions { get; set; }

    /// <summary>CiC — Commercially in Confidence.</summary>
    [FormIgnore("No page designed yet")]
    public YesNoUnknown? IndicationSpecificPricingPlanned { get; set; }

    /// <summary>Conditional on IndicationSpecificPricingPlanned = Yes.</summary>
    [FormIgnore("No page designed yet")]
    public string? IndicationSpecificPricingDetails { get; set; }

    /// <summary>
    /// Estimated net UK budget impact over the first 3 years of NHS use.
    /// </summary>
    [FormIgnore("No page designed yet")]
    public NetUkBudgetImpactBand? NetUkBudgetImpactBand { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
