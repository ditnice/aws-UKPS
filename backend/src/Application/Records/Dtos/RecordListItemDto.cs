using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Represents the record summary returned by the record list endpoint.
/// </summary>
public sealed record RecordListItemDto
{
    /// <summary>
    /// Gets the record identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the record type.
    /// </summary>
    public required RecordType RecordType { get; init; }

    /// <summary>
    /// Gets the record status.
    /// </summary>
    public required RecordStatus RecordStatus { get; init; }

    /// <summary>
    /// Gets the human-readable record title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the development name of the active substance, when available.
    /// </summary>
    public string? DevelopmentName { get; init; }

    /// <summary>
    /// Gets the date the record was last reviewed, when available.
    /// </summary>
    public DateTime? ReviewedAt { get; init; }
}
