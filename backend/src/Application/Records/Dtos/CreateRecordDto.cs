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
    /// Gets the identifier of the initial revision created for the record.
    /// </summary>
    public required int RevisionId { get; init; }

    /// <summary>
    /// Gets the identifier of the first page of the record's content form, where editing starts.
    /// </summary>
    public required string FirstPageId { get; init; }
}
