using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the budget impact section of a medicine record.
/// </summary>
public sealed record MedicinesBudgetImpactDto
{
    /// <summary>
    /// Gets whether a patient access scheme is planned.
    /// </summary>
    public YesNoUnknown? PatientAccessSchemePlanned { get; init; }

    /// <summary>
    /// Gets whether indication-specific pricing is planned.
    /// </summary>
    public YesNoUnknown? IndicationSpecificPricingPlanned { get; init; }

    /// <summary>
    /// Gets details of the indication-specific pricing.
    /// </summary>
    public string? IndicationSpecificPricingDetails { get; init; }

    /// <summary>
    /// Gets the net UK budget impact band.
    /// </summary>
    public NetUkBudgetImpactBand? NetUkBudgetImpactBand { get; init; }

    /// <summary>
    /// Gets the patient access scheme regions, or <c>null</c> if unanswered.
    /// </summary>
    public IReadOnlyCollection<PatientAccessSchemeRegion>? PatientAccessSchemeRegions { get; init; }
}
