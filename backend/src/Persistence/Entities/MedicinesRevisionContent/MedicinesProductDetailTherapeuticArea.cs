namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// Junction: up to 3 therapeutic areas per medicine product detail.
/// Parents off MedicinesProductDetail so querying that section
/// returns all associated therapeutic area selections.
/// </summary>
internal sealed class MedicinesProductDetailTherapeuticArea
{
    public int MedicinesProductDetailId { get; set; }
    public int TherapeuticAreaId { get; set; }

    // Navigation
    public MedicinesProductDetail? MedicinesProductDetail { get; set; }
    public ReferenceData.TherapeuticArea? TherapeuticArea { get; set; }
}
