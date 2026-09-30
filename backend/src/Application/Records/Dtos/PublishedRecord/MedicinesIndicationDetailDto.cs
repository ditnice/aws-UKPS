using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the indication details section of a medicine record.
/// </summary>
public sealed record MedicinesIndicationDetailDto
{
    /// <summary>
    /// Gets the indication.
    /// </summary>
    public string? Indication { get; init; }

    /// <summary>
    /// Gets the BNF chapter.
    /// </summary>
    public ReferenceDataDto? BnfChapter { get; init; }

    /// <summary>
    /// Gets the therapeutic areas, in display order.
    /// </summary>
    public required IReadOnlyCollection<ReferenceDataDto> TherapeuticAreas { get; init; }

    /// <summary>
    /// Gets whether the indication is paediatric.
    /// </summary>
    public IndicationPaediatricStatus? IndicationIsPaediatric { get; init; }

    /// <summary>
    /// Gets whether the indication is cancer.
    /// </summary>
    public YesNoUnknown? IndicationIsCancer { get; init; }

    /// <summary>
    /// Gets whether the product is intended to treat a rare disease.
    /// </summary>
    public YesNoUnknown? IndicationIsRareDisease { get; init; }

    /// <summary>
    /// Gets the formulation type.
    /// </summary>
    public ReferenceDataDto? FormulationType { get; init; }

    /// <summary>
    /// Gets the presentation.
    /// </summary>
    public string? Presentation { get; init; }

    /// <summary>
    /// Gets the mode of action.
    /// </summary>
    public string? ModeOfAction { get; init; }

    /// <summary>
    /// Gets the proposed dose regimen.
    /// </summary>
    public string? ProposedDoseRegimen { get; init; }

    /// <summary>
    /// Gets whether this is a personalised medicine.
    /// </summary>
    public YesNoUnknown? IsPersonalisedMedicine { get; init; }

    /// <summary>
    /// Gets the selected technology statuses, or <c>null</c> if unanswered.
    /// </summary>
    public IReadOnlyCollection<MedicineTechnologyStatus>? MedicineTechnologyStatus { get; init; }
}
