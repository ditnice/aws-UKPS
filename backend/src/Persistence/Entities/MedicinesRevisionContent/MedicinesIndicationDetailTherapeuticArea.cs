namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// Junction: up to 3 therapeutic areas per medicine indication detail.
/// Parents off MedicinesIndicationDetail so querying that section
/// returns all associated therapeutic area selections.
/// </summary>
internal sealed class MedicinesIndicationDetailTherapeuticArea
{
    public int MedicinesIndicationDetailId { get; set; }
    public int TherapeuticAreaId { get; set; }

    // Navigation
    public MedicinesIndicationDetail? MedicinesIndicationDetail { get; set; }
    public ReferenceData.TherapeuticArea? TherapeuticArea { get; set; }
}
