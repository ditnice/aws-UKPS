namespace UKPS.Api.Persistence.Enums;

internal static class UserOrgMembershipStatusExtensions
{
    public static UserOrgStatus ConvertToUserOrgStatus(
        this UserOrgMembershipStatus userOrgMembershipStatus
    )
    {
        return userOrgMembershipStatus switch
        {
            UserOrgMembershipStatus.AwaitingSetup => UserOrgStatus.AwaitingSetup,
            UserOrgMembershipStatus.Active => UserOrgStatus.Active,
            UserOrgMembershipStatus.Inactive => UserOrgStatus.Inactive,
            UserOrgMembershipStatus.Deactivated => UserOrgStatus.Deactivated,
            _ => throw new ArgumentOutOfRangeException(
                nameof(userOrgMembershipStatus),
                userOrgMembershipStatus,
                "Unknown user organisation membership status."
            ),
        };
    }
}
