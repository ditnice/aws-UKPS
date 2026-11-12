namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// The sections of the Medicine form, for <see cref="FormAnnotations.FormSectionAttribute"/>. A
/// section is declared on every one of its entities, and every declaration must agree on ID,
/// title and order, so each section's values are kept here in one place.
/// </summary>
/// <remarks>
/// Section IDs, titles and orders are part of the form fingerprint, so changing one needs a form
/// version bump.
/// </remarks>
internal static class MedicineFormSections
{
    /// <summary>The "Provide indication and development information" section's ID.</summary>
    public const string IndicationAndDevelopment = "indication-and-development";

    /// <summary>The "Provide indication and development information" section's title.</summary>
    public const string IndicationAndDevelopmentTitle =
        "Provide indication and development information";

    /// <summary>The "Provide indication and development information" section's position.</summary>
    public const int IndicationAndDevelopmentOrder = 10;

    /// <summary>The "Provide regulatory, access, and launch information" section's ID.</summary>
    public const string RegulatoryAccessAndLaunch = "regulatory-access-and-launch";

    /// <summary>The "Provide regulatory, access, and launch information" section's title.</summary>
    public const string RegulatoryAccessAndLaunchTitle =
        "Provide regulatory, access, and launch information";

    /// <summary>The "Provide regulatory, access, and launch information" section's position.</summary>
    public const int RegulatoryAccessAndLaunchOrder = 30;

    /// <summary>The "Provide service readiness information" section's ID.</summary>
    public const string ServiceReadiness = "service-readiness";

    /// <summary>The "Provide service readiness information" section's title.</summary>
    public const string ServiceReadinessTitle = "Provide service readiness information (optional)";

    /// <summary>The "Provide service readiness information" section's position.</summary>
    public const int ServiceReadinessOrder = 60;
}
