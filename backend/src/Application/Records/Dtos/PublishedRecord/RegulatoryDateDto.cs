using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents a regulatory milestone date.
/// </summary>
public sealed record RegulatoryDateDto
{
    /// <summary>
    /// Gets the date. Quarter and month precision dates are stored as the first day of the period.
    /// </summary>
    public required DateOnly DateValue { get; init; }

    /// <summary>
    /// Gets the precision of the date.
    /// </summary>
    public required DatePrecision DatePrecision { get; init; }

    /// <summary>
    /// Gets a value indicating whether the date is confidential.
    /// </summary>
    public required bool IsConfidential { get; init; }

    /// <summary>
    /// Gets whether conditional approval is anticipated. Only relevant for licence dates.
    /// </summary>
    public YesNoUnknown? ConditionalApprovalAnticipated { get; init; }
}
