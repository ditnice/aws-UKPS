using UKPS.Api.Persistence.Entities.Identity;

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
            UserOrgMembershipStatus.Removed => UserOrgStatus.Removed,
            _ => throw new ArgumentOutOfRangeException(
                nameof(userOrgMembershipStatus),
                userOrgMembershipStatus,
                "Unknown user organisation membership status."
            ),
        };
    }

    public static StateMachineTransitionResult<UserOrgStatus> ConvertToUserOrgStatusTransitionResult(
        this StateMachineTransitionResult<UserOrgMembershipStatus> result
    )
    {
        return new StateMachineTransitionResult<UserOrgStatus>
        {
            PreviousState = result.PreviousState.ConvertToUserOrgStatus(),
            CurrentState = result.CurrentState.ConvertToUserOrgStatus(),
            Success = result.Success,
            PermittedNextState = result
                .PermittedNextState.Select(s => s.ConvertToUserOrgStatus())
                .ToArray(),
        };
    }
}
