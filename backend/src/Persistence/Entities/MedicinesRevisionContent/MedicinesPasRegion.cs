namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>Junction table: PAS regions selected for a medicine record.</summary>
internal sealed class MedicinesPasRegion
{
    public int MedicinesBudgetImpactId { get; set; }
    public int PasRegionId { get; set; }

    // Navigation
    public MedicinesBudgetImpact? MedicinesBudgetImpact { get; set; }
    public ReferenceData.PasRegion? PasRegion { get; set; }
}
