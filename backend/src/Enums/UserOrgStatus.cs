namespace UKPS.Api.Enums;

public enum UserOrgStatus
{
    /// <summary>User/organisation has requested access and is awaiting review.</summary>
    RequestedAccess = 0,

    /// <summary>User/organisation access has been approved but setup is not complete.</summary>
    AwaitingSetup = 1,

    /// <summary>User is active within the organisation; organisation has active users.</summary>
    Active = 2,

    /// <summary>User/organisation access request was rejected.</summary>
    Rejected = 3,

    /// <summary>User is no longer active within the organisation; organisation has no active users.</summary>
    Inactive = 4,
}
