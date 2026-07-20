using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

internal sealed class MedicinesProductDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>
    /// Short human-readable label to identify this record on the homepage.
    /// e.g. Chronic hepatitis C in adults.
    /// </summary>
    public required string RecordTitle { get; set; }

    public string? BrandedName { get; set; }
    public required string Indication { get; set; }
    public IndicationPaediatricStatus? IndicationIsPaediatric { get; set; }
    public YesNoUnknown? IndicationIsCancer { get; set; }
    public int? BnfChapterId { get; set; }
    public int? TherapeuticAreaId { get; set; }
    public int? FormulationTypeId { get; set; }
    public string? Presentation { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.BnfChapter? BnfChapter { get; set; }
    public ReferenceData.TherapeuticArea? TherapeuticArea { get; set; }
    public ReferenceData.FormulationType? FormulationType { get; set; }
    public ICollection<MedicinesActiveSubstance> ActiveSubstances { get; set; } = [];
    public ICollection<MedicinesRecordStatus> RecordStatuses { get; set; } = [];
}
