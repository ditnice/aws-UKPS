using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.RecordDetails;

/// <summary>
/// Represents a name or identifier for a product.
/// </summary>
public sealed record RecordNameAndIdentifierDto
{
    /// <summary>
    /// Gets the name or identifier.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the type of the name or identifier.
    /// </summary>
    public required NameAndIdentifierType NameType { get; init; }
}
