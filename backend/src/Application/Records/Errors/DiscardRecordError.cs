namespace UKPS.Api.Application.Records.Errors;

/// <summary>
/// Represents an abstract base record for errors that can occur when discrading draft records.
/// </summary>
public abstract record DiscardRecordError
{
    /// <summary>
    /// Represents an error indicating that record cannot be found in the current database.
    /// </summary>
    internal sealed record RecordNotFound() : DiscardRecordError;

    /// <summary>
    /// Represents an error indicating that the current user cannot be found in the current database.
    /// </summary>
    internal sealed record UserNotFound() : DiscardRecordError;

    /// <summary>
    /// Represents an error indicating that the record is not suitable to be discarded.
    /// </summary>
    internal sealed record InvalidDiscard() : DiscardRecordError;
}
