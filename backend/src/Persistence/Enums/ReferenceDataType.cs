namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the types of reference data available in the system.
/// </summary>
[Flags]
public enum ReferenceDataType
{
    /// <summary>
    /// Reference data specific to medicines only.
    /// </summary>
    MedicinesOnly = 1,

    /// <summary>
    /// Reference data specific to vaccines only.
    /// </summary>
    VaccinesOnly = 2,

    /// <summary>
    /// Reference data shared between medicines and vaccines.
    /// </summary>
    Shared = MedicinesOnly | VaccinesOnly,
}
