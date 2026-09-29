namespace UKPS.Api.WebApi;

/// <summary>
/// Represents the possible reasons authentication can fail after a user has been authenticated.
/// </summary>
public enum AuthenticationFailCode
{
    /// <summary>
    /// No database user exists with the authenticated user's username.
    /// </summary>
    NoDbUserExistsWithUsername = 0,

    /// <summary>
    /// The user has no organisation memberships.
    /// </summary>
    NoMembershipsForUser = 1,

    /// <summary>
    /// A selected organisation is required to continue authentication.
    /// </summary>
    SelectedOrganisationRequired = 2,

    /// <summary>
    /// The selected organisation is not valid for the authenticated user.
    /// </summary>
    SelectedOrganisationIsNotValid = 3,

    /// <summary>
    /// The user's membership for the selected organisation has been deactivated.
    /// </summary>
    MembershipDeactivated = 4,

    /// <summary>
    /// The user's membership is not in a valid state for authentication.
    /// </summary>
    MembershipNotInValidState = 5,
}
