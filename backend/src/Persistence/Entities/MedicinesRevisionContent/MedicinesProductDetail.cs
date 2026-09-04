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

    /// <summary>
    /// Is this product intended to treat a rare disease?
    /// A disease is rare if fewer than 5 in 10,000 people have it.
    /// </summary>
    public YesNoUnknown? IndicationIsRareDisease { get; set; }

    /// <summary>
    /// The unique identifier assigned by NICE to this technology appraisal.
    /// Format: GID-TAXXXX or GID-HSTXXXX. Optional.
    /// </summary>
    public string? NiceTaDevelopmentId { get; set; }

    public int? BnfChapterId { get; set; }
    public int? FormulationTypeId { get; set; }
    public string? Presentation { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.BnfChapter? BnfChapter { get; set; }
    public ReferenceData.FormulationType? FormulationType { get; set; }

    /// <summary>
    /// Multi-select: up to 3 therapeutic areas.
    /// See MedicinesProductDetailTherapeuticArea junction table.
    /// </summary>
    public ICollection<MedicinesProductDetailTherapeuticArea> TherapeuticAreas { get; set; } = [];

    public ICollection<MedicinesActiveSubstance> ActiveSubstances { get; set; } = [];
    public ICollection<MedicinesRecordStatus> RecordStatuses { get; set; } = [];
}
