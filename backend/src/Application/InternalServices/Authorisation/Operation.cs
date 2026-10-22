namespace UKPS.Api.Application.InternalServices.Authorisation;

internal enum Operation
{
    Read = 0,
    Create = 1,
    Update = 2,
    Delete = 3,
    SignUpUser = 4,

    /// <summary>
    /// Read access but only for champion/Super users, e.g. accessing
    /// another users' details.
    /// </summary>
    ElevatedRead = 5,

    /// <summary>
    /// Answering record content form pages on a draft revision. Granted to standard users, unlike
    /// <see cref="Update"/>.
    /// </summary>
    EditRecordContent = 6,
}
