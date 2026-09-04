using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// Junction: HTA bodies a medicine is intended to be submitted to. Multi-select.
/// Vaccines answer a separate single-select question — see RecordHta.VaccineHtaAssessor.
/// </summary>
internal sealed class MedicinesHtaBody
{
    public int RecordHtaId { get; set; }
    public MedicineHtaAssessor Assessor { get; set; }

    // Navigation
    public SharedRevisionContent.RecordHta? RecordHta { get; set; }
}
