using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>Pricing and budget impact.</summary>
[FormSection(
    RecordType.Medicine,
    MedicineFormSections.ServiceReadiness,
    MedicineFormSections.ServiceReadinessTitle,
    Order = MedicineFormSections.ServiceReadinessOrder
)]
internal sealed class MedicinesBudgetImpact
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    /// <summary>Enter the estimated uptake based on expected adoption patterns. CiC.</summary>
    [FormTask(
        MedicineFormTasks.PricingAndBudgetImpact,
        MedicineFormTasks.PricingAndBudgetImpactTitle,
        Order = MedicineFormTasks.PricingAndBudgetImpactOrder
    )]
    [FormPage("estimated-uptake", "Estimated uptake", Order = 10, CommercialInConfidence = true)]
    [FormField(
        "medicines_budget_impact.estimated_uptake",
        "Enter the estimated uptake based on expected adoption patterns",
        FormControl.Textarea,
        Hint = "Give your best estimate of how many patients are likely to receive this treatment in the first few years after launch, and any factors that might affect uptake, for example, whether it replaces an existing treatment or targets a new patient group.\n\nAny information provided is a crucial help for horizon scanners to assess the likely financial and service impact ahead of HTA review."
    )]
    public string? EstimatedUptake { get; set; }

    /// <summary>
    /// Will UK patients be able to access this product on a compassionate or early access
    /// basis, outside of clinical trials?
    /// </summary>
    [FormTask(
        MedicineFormTasks.PricingAndBudgetImpact,
        MedicineFormTasks.PricingAndBudgetImpactTitle,
        Order = MedicineFormTasks.PricingAndBudgetImpactOrder
    )]
    [FormPage("compassionate-access", "Compassionate or early access", Order = 20)]
    [FormField(
        "medicines_budget_impact.compassionate_access_available",
        "Will UK patients be able to access this product on a compassionate or early access basis, outside of clinical trials?",
        FormControl.Radio,
        Order = 10
    )]
    [FormRequired(
        "Select yes if UK patients will be able to access this product on a compassionate or early access basis outside of clinical trials"
    )]
    public YesNoUnknown? CompassionateAccessAvailable { get; set; }

    /// <summary>Conditional on CompassionateAccessAvailable = Yes.</summary>
    [FormTask(
        MedicineFormTasks.PricingAndBudgetImpact,
        MedicineFormTasks.PricingAndBudgetImpactTitle,
        Order = MedicineFormTasks.PricingAndBudgetImpactOrder
    )]
    [FormPage("compassionate-access", "Compassionate or early access", Order = 20)]
    [FormField(
        "medicines_budget_impact.compassionate_access_details",
        "Provide further details",
        FormControl.Textarea,
        Order = 20,
        Hint = "Describe any schemes allowing patients to access this treatment before or outside of routine NHS commissioning, for example managed access, named patient, or expanded access schemes. Include an estimate of how many patients are currently accessing treatment this way if known, as this can significantly affect uptake estimates in the first year of routine availability."
    )]
    [FormShowWhen("medicines_budget_impact.compassionate_access_available", YesNoUnknown.Yes)]
    [FormRequired("Enter details")]
    public string? CompassionateAccessDetails { get; set; }

    /// <summary>
    /// Is a Patient Access Scheme or alternative discount arrangement planned
    /// for this indication? Gates the PatientAccessSchemeRegions selection. CiC.
    /// </summary>
    [FormTask(
        MedicineFormTasks.PricingAndBudgetImpact,
        MedicineFormTasks.PricingAndBudgetImpactTitle,
        Order = MedicineFormTasks.PricingAndBudgetImpactOrder
    )]
    [FormPage(
        "patient-access-scheme",
        "Patient Access Scheme",
        Order = 30,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_budget_impact.patient_access_scheme_planned",
        "Is a Patient Access Scheme or alternative discount arrangement planned for this indication?",
        FormControl.Radio,
        Order = 10,
        Hint = "This field is used by service planning teams to support early financial forecasting. You do not need to provide specific pricing details. An indication that a commercial arrangement is planned, or is likely to be planned, is sufficient."
    )]
    [FormRequired(
        "Select yes if a Patient Access Scheme or alternative discount arrangement is planned for this indication"
    )]
    public YesNoUnknown? PatientAccessSchemePlanned { get; set; }

    /// <summary>Multi-select. Conditional on PatientAccessSchemePlanned = Yes.</summary>
    [FormTask(
        MedicineFormTasks.PricingAndBudgetImpact,
        MedicineFormTasks.PricingAndBudgetImpactTitle,
        Order = MedicineFormTasks.PricingAndBudgetImpactOrder
    )]
    [FormPage(
        "patient-access-scheme",
        "Patient Access Scheme",
        Order = 30,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_budget_impact.patient_access_scheme_regions",
        "Select all regions where a PAS or alternative discount arrangement is planned",
        FormControl.Checkbox,
        Order = 20
    )]
    [FormShowWhen("medicines_budget_impact.patient_access_scheme_planned", YesNoUnknown.Yes)]
    [FormRequired("Select all planned locations of discount arrangements")]
    public PatientAccessSchemeRegion? PatientAccessSchemeRegions { get; set; }

    /// <summary>CiC — Commercially in Confidence.</summary>
    [FormTask(
        MedicineFormTasks.PricingAndBudgetImpact,
        MedicineFormTasks.PricingAndBudgetImpactTitle,
        Order = MedicineFormTasks.PricingAndBudgetImpactOrder
    )]
    [FormPage(
        "indication-specific-pricing",
        "Indication specific pricing",
        Order = 40,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_budget_impact.indication_specific_pricing_planned",
        "Are there plans for indication specific pricing?",
        FormControl.Radio,
        Order = 10,
        Hint = "Select yes if the medicine will have different pricing arrangements depending on indication. Select no if pricing will be uniform."
    )]
    [FormRequired("Select yes if there are plans for indication specific pricing")]
    public YesNoUnknown? IndicationSpecificPricingPlanned { get; set; }

    /// <summary>Conditional on IndicationSpecificPricingPlanned = Yes.</summary>
    [FormTask(
        MedicineFormTasks.PricingAndBudgetImpact,
        MedicineFormTasks.PricingAndBudgetImpactTitle,
        Order = MedicineFormTasks.PricingAndBudgetImpactOrder
    )]
    [FormPage(
        "indication-specific-pricing",
        "Indication specific pricing",
        Order = 40,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_budget_impact.indication_specific_pricing_details",
        "Provide further details",
        FormControl.Textarea,
        Order = 20,
        Hint = "This information helps horizon scanners estimate budget impact where pricing may differ from existing indications for this medicine. If the cost for the new indication is already known and is published elsewhere, indicate this."
    )]
    [FormShowWhen("medicines_budget_impact.indication_specific_pricing_planned", YesNoUnknown.Yes)]
    [FormRequired("Enter details")]
    public string? IndicationSpecificPricingDetails { get; set; }

    /// <summary>
    /// Estimated net UK budget impact over the first 3 years of NHS use.
    /// </summary>
    [FormTask(
        MedicineFormTasks.PricingAndBudgetImpact,
        MedicineFormTasks.PricingAndBudgetImpactTitle,
        Order = MedicineFormTasks.PricingAndBudgetImpactOrder
    )]
    [FormPage("net-budget-impact", "Net budget impact", Order = 50, CommercialInConfidence = true)]
    [FormField(
        "medicines_budget_impact.net_uk_budget_impact_band",
        "What is the estimated net budget impact for the UK over the first 3 years of NHS use?",
        FormControl.Radio,
        Hint = "Select the band that best describes the estimated net annual budget impact for the UK in the first 3 years of NHS use. Use your best estimate based on available information. Estimates can be updated as more information becomes available.\n\nAn annual impact of £40 million or more triggers specific planning processes within NICE's technology appraisal programme, so an early signal at this level is particularly valuable even if uncertain."
    )]
    [FormRequired("Select the estimated net budget impact")]
    public NetUkBudgetImpactBand? NetUkBudgetImpactBand { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
