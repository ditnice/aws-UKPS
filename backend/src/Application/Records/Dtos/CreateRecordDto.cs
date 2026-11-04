namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Represents the result of creating a record.
/// </summary>
public sealed record CreateRecordDto
{
    /// <summary>
    /// Gets the identifier of the newly created record.
    /// </summary>
    public required int RecordId { get; init; }

    /// <summary>
    /// Gets whether the record type has a content form and the caller can edit it. When true, the
    /// user goes next to the record's task list.
    /// </summary>
    public required bool CanEditContent { get; init; }
}
