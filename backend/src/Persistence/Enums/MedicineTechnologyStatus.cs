namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Technology status types selected for a medicine record. Multi-select —
/// see MedicinesProductDetail.MedicineTechnologyStatus.
/// </summary>
[Flags]
public enum MedicineTechnologyStatus
{
    /// <summary>The product is a biosimilar.</summary>
    Biosimilar = 1,

    /// <summary>The product is a new chemical or biological entity.</summary>
    NewChemicalOrBiologicalEntity = 2,

    /// <summary>The product introduces a new dosing regimen.</summary>
    NewDosingRegimen = 4,

    /// <summary>The product introduces a new formulation.</summary>
    NewFormulation = 8,

    /// <summary>The product introduces a new indication.</summary>
    NewIndication = 16,

    /// <summary>The product introduces a new presentation.</summary>
    NewPresentation = 32,

    /// <summary>
    /// An SPC (Summary of Product Characteristics) amendment without an
    /// indication change.
    /// </summary>
    SpcAmendmentWithoutIndicationChange = 64,
}
