namespace UKPS.Api.Services.Errors;

/// <summary>
/// Represents the base class for errors that can occur when updating a user's role
/// in an organisation membership.
/// </summary>
public abstract record OrganisationMembershipUpdateUserRoleError
{
    /// <summary>
    /// Represents an error that occurs when the operation is not allowed.
    /// </summary>
    /// <param name="OrganisationId">The identifier of the organisation where the operation was attempted.</param>
    internal sealed record NotAllowed(int OrganisationId)
        : OrganisationMembershipUpdateUserRoleError;

    /// <summary>
    /// Represents an error that occurs when the specified organisation or membership is not found.
    /// </summary>
    /// <param name="OrganisationId">The identifier of the organisation.</param>
    /// <param name="MembershipId">The identifier of the membership.</param>
    public sealed record NotFound(int OrganisationId, int MembershipId)
        : OrganisationMembershipUpdateUserRoleError;
}
