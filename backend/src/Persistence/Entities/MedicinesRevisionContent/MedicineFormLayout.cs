using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// The sections and tasks of the Medicine form. Each page names its task with one of the task ID
/// constants here.
/// </summary>
/// <remarks>
/// Section and task IDs, titles and orders are part of the form fingerprint, so changing one
/// needs a form version bump.
/// </remarks>
internal sealed class MedicineFormLayout()
    : FormLayout(
        RecordType.Medicine,
        [
            new FormLayoutSection(
                "indication-and-development",
                "Provide indication and development information",
                10,
                [
                    new FormLayoutTask(IndicationDetails, "Indication details", 10),
                    new FormLayoutTask(DevelopmentBackground, "Development background", 20),
                ]
            ),
            new FormLayoutSection(
                "regulatory-access-and-launch",
                "Provide regulatory, access, and launch information",
                30,
                [
                    new FormLayoutTask(
                        MhraProcedureAndDates,
                        "Medicines and Healthcare products Regulatory Agency (MHRA) procedure and dates",
                        10
                    ),
                    new FormLayoutTask(
                        HtaAndLaunch,
                        "Health Technology Assessment (HTA) and launch",
                        20
                    ),
                ]
            ),
            new FormLayoutSection(
                "special-designation",
                "Provide special designation information",
                50,
                [
                    new FormLayoutTask(
                        OrphanAndAtmp,
                        "Orphan medicine status and Advanced Therapy Medicinal Product (ATMP) classification",
                        10
                    ),
                    new FormLayoutTask(
                        PimAndEams,
                        "Promising Innovative Medicine (PIM) and Early Access to Medicines Scheme (EAMS)",
                        20
                    ),
                ]
            ),
            new FormLayoutSection(
                "service-readiness",
                "Provide service readiness information (optional)",
                60,
                [
                    new FormLayoutTask(LaboratoryTesting, "Laboratory testing", 10),
                    new FormLayoutTask(
                        PatientAndClinicalRequirements,
                        "Patient and clinical requirements",
                        20
                    ),
                    new FormLayoutTask(PricingAndBudgetImpact, "Pricing and budget impact", 30),
                ]
            ),
        ]
    )
{
    /// <summary>The "Indication details" task's ID.</summary>
    public const string IndicationDetails = "indication-details";

    /// <summary>The "Development background" task's ID.</summary>
    public const string DevelopmentBackground = "development-background";

    /// <summary>
    /// The "Medicines and Healthcare products Regulatory Agency (MHRA) procedure and dates" task's
    /// ID.
    /// </summary>
    public const string MhraProcedureAndDates = "mhra-procedure-and-dates";

    /// <summary>The "Health Technology Assessment (HTA) and launch" task's ID.</summary>
    public const string HtaAndLaunch = "hta-and-launch";

    /// <summary>
    /// The "Orphan medicine status and Advanced Therapy Medicinal Product (ATMP) classification"
    /// task's ID.
    /// </summary>
    public const string OrphanAndAtmp = "orphan-and-atmp";

    /// <summary>
    /// The "Promising Innovative Medicine (PIM) and Early Access to Medicines Scheme (EAMS)" task's
    /// ID.
    /// </summary>
    public const string PimAndEams = "pim-and-eams";

    /// <summary>The "Laboratory testing" task's ID.</summary>
    public const string LaboratoryTesting = "laboratory-testing";

    /// <summary>The "Patient and clinical requirements" task's ID.</summary>
    public const string PatientAndClinicalRequirements = "patient-and-clinical-requirements";

    /// <summary>The "Pricing and budget impact" task's ID.</summary>
    public const string PricingAndBudgetImpact = "pricing-and-budget-impact";
}
