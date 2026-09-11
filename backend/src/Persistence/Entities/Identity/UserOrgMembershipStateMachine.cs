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
            }
        );

        ForState(
            UserOrgMembershipStatus.Active,
            x =>
            {
                x.On(Command.MarkedAsInactive, UserOrgMembershipStatus.Inactive);
                x.On(Command.Deactivate, UserOrgMembershipStatus.Deactivated);
                x.Ignore(Command.Reactivate);
            }
        );

        ForState(
            UserOrgMembershipStatus.Inactive,
            x =>
            {
                x.On(Command.Deactivate, UserOrgMembershipStatus.Deactivated);
                x.On(Command.MarkedAsActive, UserOrgMembershipStatus.Active);
            }
        );

        ForState(
            UserOrgMembershipStatus.Deactivated,
            x =>
            {
                x.On(Command.Reactivate, UserOrgMembershipStatus.Active);
                x.Ignore(Command.Deactivate);
            }
        );
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
    }
}
