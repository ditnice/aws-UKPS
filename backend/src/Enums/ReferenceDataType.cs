namespace UKPS.Api.Enums;

/// <summary>
/// Represents the types of reference data available in the system.
/// </summary>
public enum ReferenceDataType
{
    /// <summary>
    /// Reference data specific to medicines only.
    /// </summary>
    MedicinesOnly = 0,

    /// <summary>
    /// Reference data specific to vaccines only.
    /// </summary>
    VaccinesOnly = 1,

    /// <summary>
    /// Reference data shared between medicines and vaccines.
    /// </summary>
    Shared = 2,
}
