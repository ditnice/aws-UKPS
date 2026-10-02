namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Whether testing is required before treatment can proceed.
/// Replaces GenomicTestMandatory on MedicinesLaboratoryTesting.
/// </summary>
public enum GenomicTestMandatoryStatus
{
    /// <summary>Whether testing is mandatory is not yet known.</summary>
    Unknown = 0,

    /// <summary>Testing is recommended but not required.</summary>
    RecommendedNotRequired = 1,

    /// <summary>Testing is mandatory, but suitable alternatives may exist.</summary>
    MandatoryAlternativesMayExist = 2,

    /// <summary>Testing is mandatory; treatment cannot proceed without a positive result.</summary>
    MandatoryNoAlternatives = 3,
}
