namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Specifies the fields by which users can be sorted when querying users.
/// </summary>
public enum GetUsersQuerySortValue
{
    /// <summary>
    /// Sorts users by their last active timestamp.
    /// </summary>
    LastActive = 0,

    /// <summary>
    /// Sorts users by their email address.
    /// </summary>
    Email = 1,

    /// <summary>
    /// Sorts users by their role.
    /// </summary>
    Role = 2,

    /// <summary>
    /// Sorts users by their organisation status.
    /// </summary>
    Status = 3,
}
