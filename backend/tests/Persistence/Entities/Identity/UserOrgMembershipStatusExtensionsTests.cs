using Shouldly;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Persistence.Entities.Identity;

public sealed class UserOrgMembershipStatusExtensionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConvertToUserOrgStatusTransitionResult_ShouldConvertResult(bool success)
    {
        var result = new StateMachineTransitionResult<UserOrgMembershipStatus>
        {
            PreviousState = UserOrgMembershipStatus.Active,
            CurrentState = UserOrgMembershipStatus.Deactivated,
            Success = success,
            PermittedNextState = [UserOrgMembershipStatus.Active, UserOrgMembershipStatus.Removed],
        };

        var converted = result.ConvertToUserOrgStatusTransitionResult();

        converted.PreviousState.ShouldBe(UserOrgStatus.Active);
        converted.CurrentState.ShouldBe(UserOrgStatus.Deactivated);
        converted.Success.ShouldBe(success);
        converted.PermittedNextState.ShouldBe([UserOrgStatus.Active, UserOrgStatus.Removed]);
    }

    [Theory]
    [InlineData((int)UserOrgMembershipStatus.AwaitingSetup, UserOrgStatus.AwaitingSetup)]
    [InlineData((int)UserOrgMembershipStatus.Active, UserOrgStatus.Active)]
    [InlineData((int)UserOrgMembershipStatus.Inactive, UserOrgStatus.Inactive)]
    [InlineData((int)UserOrgMembershipStatus.Deactivated, UserOrgStatus.Deactivated)]
    [InlineData((int)UserOrgMembershipStatus.Removed, UserOrgStatus.Removed)]
    public void ConvertToUserOrgStatus_ShouldConvertStatus(int status, UserOrgStatus expected)
    {
        ((UserOrgMembershipStatus)status).ConvertToUserOrgStatus().ShouldBe(expected);
    }
}
