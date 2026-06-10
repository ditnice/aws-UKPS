using UKPS.Api.Enums;

namespace UKPS.Api.Entities.MedicinesRevisionContent;

/// <summary>Junction table: technology status types selected for a medicine record.</summary>
public class MedicinesRecordStatus
{
    public int MedicinesProductDetailId { get; set; }
    public int MedicineStatusTypeId { get; set; }

    // Navigation
    public MedicinesProductDetail MedicinesProductDetail { get; set; } = null!;
    public ReferenceData.MedicineTechnologyStatus MedicineStatusType { get; set; } = null!;
}
