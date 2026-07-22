namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>Junction table: technology status types selected for a medicine record.</summary>
internal sealed class MedicinesRecordStatus
{
    public int MedicinesProductDetailId { get; set; }
    public int MedicineStatusTypeId { get; set; }

    // Navigation
    public MedicinesProductDetail? MedicinesProductDetail { get; set; }
    public ReferenceData.MedicineTechnologyStatus? MedicineStatusType { get; set; }
}
