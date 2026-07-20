namespace UKPS.Api.Services.Errors;

/// <summary>
/// Represents the base class for errors that can occur when attempting to deactivate a user
/// in an organisation membership context.
/// </summary>
public abstract record OrganisationMembershipDeactivateUserError
{
    /// <summary>
    /// Represents an error indicating that the operation is not allowed for the specified organisation.
    /// </summary>
    /// <param name="OrganisationId">The identifier of the organisation where the operation is not allowed.</param>
    internal sealed record NotAllowed(int OrganisationId)
        : OrganisationMembershipDeactivateUserError;

    /// <summary>
    /// Represents an error indicating that the specified organisation or user was not found.
    /// </summary>
    public sealed record NotFound() : OrganisationMembershipDeactivateUserError;
}
