namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents a selected reference data value.
/// </summary>
public sealed record ReferenceDataDto
{
    /// <summary>
    /// Gets the reference data identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the display label.
    /// </summary>
    public required string Label { get; init; }
}
