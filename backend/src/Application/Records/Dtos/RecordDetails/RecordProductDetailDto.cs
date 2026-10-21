namespace UKPS.Api.Application.Records.Dtos.RecordDetails;

/// <summary>
/// Represents the names and identifiers section of a record.
/// </summary>
public sealed record RecordProductDetailDto
{
    /// <summary>
    /// Gets the company code.
    /// </summary>
    public required string CompanyCode { get; init; }

    /// <summary>
    /// Gets the generic names and other identifiers, in display order.
    /// </summary>
    public required IReadOnlyCollection<RecordNameAndIdentifierDto> NamesAndIdentifiers { get; init; }

    /// <summary>
    /// Gets the branded name.
    /// </summary>
    public string? BrandedName { get; init; }

    /// <summary>
    /// Gets the short human-readable label identifying the record.
    /// </summary>
    public required string RecordTitle { get; init; }
}
