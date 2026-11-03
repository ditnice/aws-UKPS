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
    public const int DevelopmentBackgroundOrder = 10;
}
