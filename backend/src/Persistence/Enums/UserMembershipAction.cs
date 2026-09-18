namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Defines the actions that can be performed on a user's membership.
/// </summary>
public enum UserMembershipAction
{
    /// <summary>
    /// Approves the user's membership.
    /// </summary>
    ApproveMembership = 0,

    /// <summary>
    /// Rejects the user's membership.
    /// </summary>
    RejectMembership = 1,

    /// <summary>
    /// Deactivates the user's membership.
    /// </summary>
    DeactivateMembership = 2,

    /// <summary>
    /// Reactivates the user's membership.
    /// </summary>
    ReactivateMembership = 3,

    /// <summary>
    /// Allows the user's role to be edited.
    /// </summary>
    EditUserRole = 4,
}
