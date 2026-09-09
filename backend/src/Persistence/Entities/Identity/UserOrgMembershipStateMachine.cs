using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserOrgMembershipStateMachine
    : StateMachine<UserOrgStatus, UserOrgMembershipStateMachine.Command>
{
    public UserOrgMembershipStateMachine(UserOrgStatus initialState)
        : base(initialState)
    {
        ForState(
            UserOrgStatus.RequestedAccess,
            x =>
            {
                x.On(Command.AccessGranted, UserOrgStatus.AwaitingSetup);
                x.On(Command.RequestRejected, UserOrgStatus.Rejected);
            }
        );

        ForState(
            UserOrgStatus.AwaitingSetup,
            x =>
            {
                x.On(Command.FinaliseSetup, UserOrgStatus.Active);
            }
        );

        ForState(
            UserOrgStatus.Active,
            x =>
            {
                x.On(Command.MarkedAsInactive, UserOrgStatus.Inactive);
                x.On(Command.Deactivate, UserOrgStatus.Deactivated);
                x.Ignore(Command.Reactivate);
            }
        );

        ForState(
            UserOrgStatus.Inactive,
            x =>
            {
                x.On(Command.Deactivate, UserOrgStatus.Deactivated);
                x.On(Command.MarkedAsActive, UserOrgStatus.Active);
            }
        );

        ForState(
            UserOrgStatus.Deactivated,
            x =>
            {
                x.On(Command.Reactivate, UserOrgStatus.Active);
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
