using UKPS.Data.Enums;

namespace UKPS.Data.Entities.MedicinesRevisionContent;

/// <summary>Junction table: PAS regions selected for a medicine record.</summary>
public class MedicinesPasRegion
{
    public int MedicinesBudgetImpactId { get; set; }
    public int PasRegionId { get; set; }

    // Navigation
    public MedicinesBudgetImpact MedicinesBudgetImpact { get; set; } = null!;
    public ReferenceData.PasRegion PasRegion { get; set; } = null!;
}
