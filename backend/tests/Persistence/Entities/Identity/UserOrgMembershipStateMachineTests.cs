using Shouldly;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Persistence.Entities.Identity;

public sealed class UserOrgMembershipStateMachineTests
{
    [Fact]
    public void TrySendCommand_ShouldTransitionToRemoved_WhenAwaitingSetup() =>
        AssertRemoveSucceeds(UserOrgMembershipStatus.AwaitingSetup);

    [Fact]
    public void TrySendCommand_ShouldTransitionToRemoved_WhenActive() =>
        AssertRemoveSucceeds(UserOrgMembershipStatus.Active);

    [Fact]
    public void TrySendCommand_ShouldTransitionToRemoved_WhenInactive() =>
        AssertRemoveSucceeds(UserOrgMembershipStatus.Inactive);

    [Fact]
    public void TrySendCommand_ShouldTransitionToRemoved_WhenDeactivated() =>
        AssertRemoveSucceeds(UserOrgMembershipStatus.Deactivated);

    [Fact]
    public void TrySendCommand_ShouldRejectRemove_WhenAlreadyRemoved()
    {
        var stateMachine = new UserOrgMembershipStateMachine(UserOrgMembershipStatus.Removed);

        var result = stateMachine.TrySendCommand(UserOrgMembershipStateMachine.Command.Remove);

        result.Success.ShouldBeFalse();
        result.CurrentState.ShouldBe(UserOrgMembershipStatus.Removed);
    }

    [Fact]
    public void TrySendCommand_ShouldRejectEveryCommand_WhenAlreadyRemoved()
    {
        foreach (
            UserOrgMembershipStateMachine.Command command in Enum.GetValues<UserOrgMembershipStateMachine.Command>()
        )
        {
            var stateMachine = new UserOrgMembershipStateMachine(UserOrgMembershipStatus.Removed);

            var result = stateMachine.TrySendCommand(command);

            result.Success.ShouldBeFalse();
            result.CurrentState.ShouldBe(UserOrgMembershipStatus.Removed);
        }
    }

    private static void AssertRemoveSucceeds(UserOrgMembershipStatus initialState)
    {
        var stateMachine = new UserOrgMembershipStateMachine(initialState);

        var result = stateMachine.TrySendCommand(UserOrgMembershipStateMachine.Command.Remove);

        result.Success.ShouldBeTrue();
        result.PreviousState.ShouldBe(initialState);
        result.CurrentState.ShouldBe(UserOrgMembershipStatus.Removed);
    }
}
