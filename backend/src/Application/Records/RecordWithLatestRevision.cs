using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Application.Records;

/// <summary>
/// Represents a record with details of its latest revision, for use in database queries.
/// </summary>
internal sealed record RecordWithLatestRevision
{
    /// <summary>
    /// Gets the record.
    /// </summary>
    public required Record Record { get; init; }

    /// <summary>
    /// Gets the identifier of the latest revision, or <c>null</c> if the record has none.
    /// </summary>
    public int? LatestRevisionId { get; init; }

    /// <summary>
    /// Gets the status shown to users for the record.
    /// </summary>
    public RecordDisplayStatus DisplayStatus { get; init; }
}
