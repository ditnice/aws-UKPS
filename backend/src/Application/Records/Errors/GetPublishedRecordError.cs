using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Errors;

/// <summary>
/// Represents the base class for errors that can occur when retrieving a published record.
/// </summary>
public abstract record GetPublishedRecordError
{
    /// <summary>
    /// Initialises a new instance of the <see cref="GetPublishedRecordError"/> class.
    /// </summary>
    private protected GetPublishedRecordError() { }

    /// <summary>
    /// Represents an error that occurs when the record does not exist or has no published data.
    /// </summary>
    /// <param name="RecordId">The ID of the record.</param>
    public sealed record NotFound(int RecordId) : GetPublishedRecordError;

    /// <summary>
    /// Represents an error that occurs when the caller cannot read the record.
    /// </summary>
    /// <param name="RecordId">The ID of the record.</param>
    internal sealed record NotAllowed(int RecordId) : GetPublishedRecordError;

    /// <summary>
    /// Represents an error that occurs when the record is not of the requested type.
    /// </summary>
    /// <param name="RecordId">The ID of the record.</param>
    /// <param name="RequestedRecordType">The record type that was requested.</param>
    /// <param name="ActualRecordType">The type of the record.</param>
    public sealed record RecordTypeMismatch(
        int RecordId,
        RecordType RequestedRecordType,
        RecordType ActualRecordType
    ) : GetPublishedRecordError;
}
