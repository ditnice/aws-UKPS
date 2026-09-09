namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// How the required genomic test relates to what is currently in the
/// National Genomic Test Directory (NGTD).
/// Replaces GenomicTestInNationalDirectory on MedicinesLaboratoryTesting.
/// </summary>
public enum GenomicTestNgtdRelationship
{
    /// <summary>The relationship to the NGTD is not yet known.</summary>
    Unknown = 0,

    /// <summary>The test is new and is not currently in the NGTD.</summary>
    NewTest = 1,

    /// <summary>The test already exists in the NGTD, but for a new indication.</summary>
    ExistingTestNewIndication = 2,

    /// <summary>The test already exists in the NGTD for the same indication.</summary>
    ExistingTestSameIndication = 3,
}
