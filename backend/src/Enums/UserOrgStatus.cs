namespace UKPS.Api.Enums;

public enum UserOrgStatus
{
    /// <summary>User has requested access and is awaiting review.</summary>
    RequestedAccess = 0,

    /// <summary>User access has been approved but setup is not complete.</summary>
    AwaitingSetup = 1,

    /// <summary>User is active within the organisation.</summary>
    Active = 2,

    /// <summary>User access request was rejected.</summary>
    Rejected = 3,

    /// <summary>User is no longer active within the organisation.</summary>
    Inactive = 4,
}
