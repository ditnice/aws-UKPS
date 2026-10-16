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
    /// Edit the content of a draft record revision. Unlike <see cref="Update"/>,
    /// this is granted to standard users.
    /// </summary>
    EditRecordContent = 6,
}
