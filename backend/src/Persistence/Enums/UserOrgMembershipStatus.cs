namespace UKPS.Api.Persistence.Enums;

internal enum UserOrgMembershipStatus
{
    /// <summary>User/organisation access has been approved but setup is not complete.</summary>
    AwaitingSetup = 1,

    /// <summary>User is active within the organisation; organisation has active users.</summary>
    Active = 2,

    /// <summary>User is no longer active within the organisation; organisation has no active users.</summary>
    Inactive = 4,

    /// <summary>User has been deactivated.</summary>
    Deactivated = 5,
}
