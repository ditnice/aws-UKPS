using UKPS.Api.Enums;

namespace UKPS.Api.Entities.MedicinesRevisionContent;

internal sealed class MedicinesBudgetImpact
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>CiC — Commercially in Confidence.</summary>
    public YesNoUnknown? IndicationSpecificPricingPlanned { get; set; }

    /// <summary>Conditional on IndicationSpecificPricingPlanned = Yes.</summary>
    public string? IndicationSpecificPricingDetails { get; set; }

    /// <summary>CiC — Commercially in Confidence.</summary>
    public YesNoUnknown? NetUkBudgetImpactOver5M { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ICollection<MedicinesPasRegion> PasRegions { get; set; } = [];
}
