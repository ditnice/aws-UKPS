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
    /// Answering record content form pages on a draft revision. Granted to standard and champion
    /// users for their own organisation, unlike <see cref="Update"/>. Never granted to super users.
    /// </summary>
    EditRecordContent = 6,
}
