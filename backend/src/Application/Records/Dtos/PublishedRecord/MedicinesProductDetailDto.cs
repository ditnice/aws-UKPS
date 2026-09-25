using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the product detail section of a medicine record.
/// </summary>
public sealed record MedicinesProductDetailDto
{
    /// <summary>
    /// Gets the short human-readable label identifying the record.
    /// </summary>
    public required string RecordTitle { get; init; }

    /// <summary>
    /// Gets the branded name.
    /// </summary>
    public string? BrandedName { get; init; }

    /// <summary>
    /// Gets the indication.
    /// </summary>
    public required string Indication { get; init; }

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
    /// Gets the NICE technology appraisal development identifier.
    /// </summary>
    public string? NiceTaDevelopmentId { get; init; }

    /// <summary>
    /// Gets the BNF chapter.
    /// </summary>
    public ReferenceDataDto? BnfChapter { get; init; }

    /// <summary>
    /// Gets the formulation type.
    /// </summary>
    public ReferenceDataDto? FormulationType { get; init; }

    /// <summary>
    /// Gets the presentation.
    /// </summary>
    public string? Presentation { get; init; }

    /// <summary>
    /// Gets the selected technology statuses, or <c>null</c> if unanswered.
    /// </summary>
    public IReadOnlyCollection<MedicineTechnologyStatus>? MedicineTechnologyStatus { get; init; }

    /// <summary>
    /// Gets the therapeutic areas, in display order.
    /// </summary>
    public required IReadOnlyCollection<ReferenceDataDto> TherapeuticAreas { get; init; }

    /// <summary>
    /// Gets the active substance names, in display order.
    /// </summary>
    public required IReadOnlyCollection<MedicinesActiveSubstanceDto> ActiveSubstances { get; init; }
}
