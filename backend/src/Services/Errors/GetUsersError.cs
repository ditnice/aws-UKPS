namespace UKPS.Api.Services.Errors;

/// <summary>
/// Represents an abstract base record for errors that can occur when retrieving users.
/// </summary>
public abstract record GetUsersError
{
    /// <summary>
    /// Represents an error indicating that the operation is not allowed for the specified organisation.
    /// </summary>
    /// <param name="OrganisationId">The identifier of the organisation for which the operation is not allowed.</param>
    internal sealed record NotAllowed(int OrganisationId) : GetUsersError;

    /// <summary>
    /// Represents an error indicating that the specified organisation was not found.
    /// </summary>
    /// <param name="OrganisationId">The identifier of the organisation that was not found.</param>
    public sealed record OrganisationNotFound(int OrganisationId) : GetUsersError;
}
