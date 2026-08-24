namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an abstract base record for errors that can occur when retrieving users.
/// </summary>
public abstract record GetUserDetailsError
{
    /// <summary>
    /// Represents an error indicating that the specified email was not found.
    /// </summary>
    /// <param name="Id">The identifier of the organisation that was not found.</param>
    public sealed record IdNotFound(int Id) : GetUserDetailsError;
}
