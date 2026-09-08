namespace UKPS.Api.Application.Organisations.Errors;

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

    /// <summary>
    /// Represents an error that occurs when a user attempts to change their own role.
    /// </summary>
    /// <param name="MembershipId">The identifier of the caller's own membership.</param>
    public sealed record CannotChangeOwnRole(int MembershipId)
        : OrganisationMembershipUpdateUserRoleError;

    /// <summary>
    /// Represents an error that occurs when a non-super user attempts to assign or modify the
    /// super user role.
    /// </summary>
    /// <param name="MembershipId">The identifier of the protected membership.</param>
    public sealed record CannotManageSuperRole(int MembershipId)
        : OrganisationMembershipUpdateUserRoleError;
}
