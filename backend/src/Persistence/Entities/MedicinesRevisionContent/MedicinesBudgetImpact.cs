using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

internal sealed class MedicinesBudgetImpact
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>
    /// Is a Patient Access Scheme or alternative discount arrangement planned
    /// for this indication? Gates the PasRegions selection. CiC.
    /// </summary>
    public YesNoUnknown? PasSchemePlanned { get; set; }

    /// <summary>CiC — Commercially in Confidence.</summary>
    public YesNoUnknown? IndicationSpecificPricingPlanned { get; set; }

    /// <summary>Conditional on IndicationSpecificPricingPlanned = Yes.</summary>
    public string? IndicationSpecificPricingDetails { get; set; }

    /// <summary>
    /// Estimated net UK budget impact over the first 3 years of NHS use.
    /// </summary>
    public NetUkBudgetImpactBand? NetUkBudgetImpactBand { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ICollection<MedicinesPasRegion> PasRegions { get; set; } = [];
}
