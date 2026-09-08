using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserOrgMembership
{
    public int Id { get; set; }
    public required UserRole UserRole { get; set; }
    public required UserOrgStatus Status
    {
        get => _statusManager.State;
        init => _statusManager = new UserOrgMembershipStateMachine(value);
    }
    public required PharmaceuticalEntity AllowedPharmaceuticalEntity { get; set; }
    public required DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
    public int OrganisationId { get; set; }
    public Organisation? Organisation { get; set; }

    private UserOrgMembershipStateMachine _statusManager = new UserOrgMembershipStateMachine(
        UserOrgStatus.AwaitingSetup
    );

    internal StateMachineTransitionResult<UserOrgStatus> TryFinaliseSetup() =>
        _statusManager.TrySendCommand(UserOrgMembershipStateMachine.Command.FinaliseSetup);

    internal void FinaliseSetup() =>
        _statusManager.SendCommand(UserOrgMembershipStateMachine.Command.FinaliseSetup);

    internal StateMachineTransitionResult<UserOrgStatus> TryDeactivate() =>
        _statusManager.TrySendCommand(UserOrgMembershipStateMachine.Command.Deactivate);

    internal void Deactivate() =>
        _statusManager.SendCommand(UserOrgMembershipStateMachine.Command.Deactivate);

    internal StateMachineTransitionResult<UserOrgStatus> TryReactivate() =>
        _statusManager.TrySendCommand(UserOrgMembershipStateMachine.Command.Reactivate);

    internal bool IsAuthorised()
    {
        UserOrgStatus[] authorisedStatuses = [UserOrgStatus.Active, UserOrgStatus.Inactive];
        return authorisedStatuses.Contains(Status);
    }

    public IEnumerable<UserMembershipAction> GetPermittedActions(
        CognitoUsername currentUserUsername,
        UserRole currentUserRole
    )
    {
        if (User is null)
        {
            throw new InvalidOperationException(
                "The User navigation property must be loaded before calling GetPermittedActions."
            );
        }

        if (currentUserUsername == User.CognitoUsername)
        {
            return [];
        }

        if (currentUserRole is not (UserRole.Champion or UserRole.Super))
        {
            return [];
        }

        UserMembershipAction[] nonCommandRelatedActions =
            Status == UserOrgStatus.Active || Status == UserOrgStatus.Inactive
                ? [UserMembershipAction.EditUserRole]
                : [];
        IEnumerable<UserOrgMembershipStateMachine.Command> permittedCommands =
            _statusManager.GetPermittedStateChangingCommands();
        return GetActionsFromPermittedCommands(permittedCommands).Concat(nonCommandRelatedActions);
    }

    private static IEnumerable<UserMembershipAction> GetActionsFromPermittedCommands(
        IEnumerable<UserOrgMembershipStateMachine.Command> permittedCommands
    )
    {
        return permittedCommands.Select(ConvertCommandToAction).OfType<UserMembershipAction>();
    }

    private static UserMembershipAction? ConvertCommandToAction(
        UserOrgMembershipStateMachine.Command x
    )
    {
        return x switch
        {
            UserOrgMembershipStateMachine.Command.AccessGranted =>
                UserMembershipAction.ApproveMembership,
            UserOrgMembershipStateMachine.Command.RequestRejected =>
                UserMembershipAction.RejectMembership,
            UserOrgMembershipStateMachine.Command.Deactivate =>
                UserMembershipAction.DeactivateMembership,
            UserOrgMembershipStateMachine.Command.Reactivate =>
                UserMembershipAction.ReactivateMembership,
            _ => null,
        };
    }
}
