namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// The tasks of the Medicine form, for <see cref="FormAnnotations.FormTaskAttribute"/>. A task is
/// declared on every one of its question properties, and every declaration must agree on ID,
/// title and order, so each task's values are kept here in one place.
/// </summary>
/// <remarks>
/// Task IDs, titles and orders are part of the form fingerprint, so changing one needs a form
/// version bump.
/// </remarks>
internal static class MedicineFormTasks
{
    /// <summary>The "Indication details" task's ID.</summary>
    public const string IndicationDetails = "indication-details";

    /// <summary>The "Indication details" task's title.</summary>
    public const string IndicationDetailsTitle = "Indication details";

    /// <summary>The "Indication details" task's position within its section.</summary>
    public const int IndicationDetailsOrder = 10;

    /// <summary>The "Development background" task's ID.</summary>
    public const string DevelopmentBackground = "development-background";

    /// <summary>The "Development background" task's title.</summary>
    public const string DevelopmentBackgroundTitle = "Development background";

    /// <summary>The "Development background" task's position within its section.</summary>
    public const int DevelopmentBackgroundOrder = 20;

    /// <summary>The "Laboratory testing" task's ID.</summary>
    public const string LaboratoryTesting = "laboratory-testing";

    /// <summary>The "Laboratory testing" task's title.</summary>
    public const string LaboratoryTestingTitle = "Laboratory testing";

    /// <summary>The "Laboratory testing" task's position within its section.</summary>
    public const int LaboratoryTestingOrder = 10;

    /// <summary>The "Health Technology Assessment (HTA) and launch" task's ID.</summary>
    public const string HtaAndLaunch = "hta-and-launch";

    /// <summary>The "Health Technology Assessment (HTA) and launch" task's title.</summary>
    public const string HtaAndLaunchTitle = "Health Technology Assessment (HTA) and launch";

    /// <summary>The "Health Technology Assessment (HTA) and launch" task's position within its section.</summary>
    public const int HtaAndLaunchOrder = 10;

    /// <summary>The "Patient and clinical requirements" task's ID.</summary>
    public const string PatientAndClinicalRequirements = "patient-and-clinical-requirements";

    /// <summary>The "Patient and clinical requirements" task's title.</summary>
    public const string PatientAndClinicalRequirementsTitle = "Patient and clinical requirements";

    /// <summary>The "Patient and clinical requirements" task's position within its section.</summary>
    public const int PatientAndClinicalRequirementsOrder = 20;

    /// <summary>The "Pricing and budget impact" task's ID.</summary>
    public const string PricingAndBudgetImpact = "pricing-and-budget-impact";

    /// <summary>The "Pricing and budget impact" task's title.</summary>
    public const string PricingAndBudgetImpactTitle = "Pricing and budget impact";

    /// <summary>The "Pricing and budget impact" task's position within its section.</summary>
    public const int PricingAndBudgetImpactOrder = 30;
}
