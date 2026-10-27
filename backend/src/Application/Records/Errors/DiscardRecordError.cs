namespace UKPS.Api.Application.Records.Errors;

/// <summary>
/// Represents the base class for errors that can occur when discarding a draft record.
/// </summary>
public abstract record DiscardRecordError
{
    /// <summary>
    /// Initialises a new instance of the <see cref="DiscardRecordError"/> class.
    /// </summary>
    private protected DiscardRecordError() { }

    /// <summary>
    /// Represents an error that occurs when the record does not exist or has been discarded.
    /// </summary>
    internal sealed record RecordNotFound() : DiscardRecordError;

    /// <summary>
    /// Represents an error that occurs when the caller cannot discard records for the record's
    /// organisation.
    /// </summary>
    internal sealed record NotAllowed() : DiscardRecordError;

    /// <summary>
    /// Represents an error that occurs when the record has been submitted, so cannot be discarded.
    /// </summary>
    internal sealed record InvalidDiscard() : DiscardRecordError;

    /// <summary>
    /// Represents an error that occurs when the record was updated by another operation while it
    /// was being discarded.
    /// </summary>
    internal sealed record ConcurrencyError() : DiscardRecordError;
}
