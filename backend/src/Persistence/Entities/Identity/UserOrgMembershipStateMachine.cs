using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserOrgMembershipStateMachine
    : StateMachine<UserOrgMembershipStatus, UserOrgMembershipStateMachine.Command>
{
    public UserOrgMembershipStateMachine(UserOrgMembershipStatus initialState)
        : base(initialState)
    {
        ForState(
            UserOrgMembershipStatus.AwaitingSetup,
            x =>
            {
                x.On(Command.FinaliseSetup, UserOrgMembershipStatus.Active);
                x.On(Command.Remove, UserOrgMembershipStatus.Removed);
            }
        );

        ForState(
            UserOrgMembershipStatus.Active,
            x =>
            {
                x.On(Command.MarkedAsInactive, UserOrgMembershipStatus.Inactive);
                x.On(Command.Deactivate, UserOrgMembershipStatus.Deactivated);
                x.On(Command.Remove, UserOrgMembershipStatus.Removed);
                x.Ignore(Command.Reactivate);
            }
        );

        ForState(
            UserOrgMembershipStatus.Inactive,
            x =>
            {
                x.On(Command.Deactivate, UserOrgMembershipStatus.Deactivated);
                x.On(Command.MarkedAsActive, UserOrgMembershipStatus.Active);
                x.On(Command.Remove, UserOrgMembershipStatus.Removed);
            }
        );

        ForState(
            UserOrgMembershipStatus.Deactivated,
            x =>
            {
                x.On(Command.Reactivate, UserOrgMembershipStatus.Active);
                x.On(Command.Remove, UserOrgMembershipStatus.Removed);
                x.Ignore(Command.Deactivate);
            }
        );

        // Removed is terminal: no transitions are defined out of it, so any command
        // sent while in this state (including Remove itself) is rejected.
        ForState(UserOrgMembershipStatus.Removed, x => { });
    }

    internal enum Command
    {
        AccessGranted = 0,
        RequestRejected = 1,
        FinaliseSetup = 2,
        Deactivate = 3,
        Reactivate = 4,
        MarkedAsInactive = 5,
        MarkedAsActive = 6,
        Remove = 7,
    }
}
