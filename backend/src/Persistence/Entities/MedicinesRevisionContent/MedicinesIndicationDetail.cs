using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

internal sealed class MedicinesIndicationDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public required string Indication { get; set; }
    public int? BnfChapterId { get; set; }
    public IndicationPaediatricStatus? IndicationIsPaediatric { get; set; }
    public YesNoUnknown? IndicationIsCancer { get; set; }

    /// <summary>
    /// Is this product intended to treat a rare disease?
    /// A disease is rare if fewer than 5 in 10,000 people have it.
    /// </summary>
    public YesNoUnknown? IndicationIsRareDisease { get; set; }

    public int? FormulationTypeId { get; set; }
    public string? Presentation { get; set; }
    public string? ModeOfAction { get; set; }
    public string? ProposedDoseRegimen { get; set; }
    public YesNoUnknown? IsPersonalisedMedicine { get; set; }

    /// <summary>Multi-select technology status types for this record.</summary>
    public MedicineTechnologyStatus? MedicineTechnologyStatus { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.BnfChapter? BnfChapter { get; set; }
    public ReferenceData.FormulationType? FormulationType { get; set; }

    /// <summary>
    /// Multi-select: up to 3 therapeutic areas.
    /// See MedicinesIndicationDetailTherapeuticArea junction table.
    /// </summary>
    public ICollection<MedicinesIndicationDetailTherapeuticArea> TherapeuticAreas { get; set; } =
    [];
}
