namespace UKPS.Api.Application.Records.Errors;

/// <summary>
/// Represents the base class for errors that can occur when changing a record's status.
/// </summary>
public abstract record ChangeRecordStatusError
{
    /// <summary>
    /// Initialises a new instance of the <see cref="ChangeRecordStatusError"/> class.
    /// </summary>
    private protected ChangeRecordStatusError() { }

    /// <summary>
    /// The record does not exist in the requested organisation, or has been discarded.
    /// </summary>
    internal sealed record NotFound() : ChangeRecordStatusError;

    /// <summary>
    /// The caller cannot read the requested organisation's records.
    /// </summary>
    internal sealed record NotAllowed() : ChangeRecordStatusError;

    /// <summary>
    /// The caller can read the record but cannot change its status.
    /// </summary>
    internal sealed record CannotChangeStatus() : ChangeRecordStatusError;

    /// <summary>
    /// The reason is not one a user can give for this status change.
    /// </summary>
    internal sealed record InvalidReason() : ChangeRecordStatusError;

    /// <summary>
    /// The reason needs a note giving the detail, and none was given.
    /// </summary>
    internal sealed record NoteRequired() : ChangeRecordStatusError;

    /// <summary>
    /// The record's current state does not permit this status change.
    /// </summary>
    internal sealed record TransitionNotPermitted() : ChangeRecordStatusError;

    /// <summary>
    /// The record was updated by another operation while its status was being changed.
    /// </summary>
    internal sealed record ConcurrencyError() : ChangeRecordStatusError;
}
