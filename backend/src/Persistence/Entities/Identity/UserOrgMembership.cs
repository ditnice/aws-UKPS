using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserOrgMembership
{
    public int Id { get; set; }
    public required UserRole UserRole { get; set; }
    public required UserOrgMembershipStatus Status
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
        UserOrgMembershipStatus.AwaitingSetup
    );

    internal StateMachineTransitionResult<UserOrgMembershipStatus> TryFinaliseSetup() =>
        _statusManager.TrySendCommand(UserOrgMembershipStateMachine.Command.FinaliseSetup);

    internal void FinaliseSetup() =>
        _statusManager.SendCommand(UserOrgMembershipStateMachine.Command.FinaliseSetup);

    internal StateMachineTransitionResult<UserOrgMembershipStatus> TryDeactivate() =>
        _statusManager.TrySendCommand(UserOrgMembershipStateMachine.Command.Deactivate);

    internal void Deactivate() =>
        _statusManager.SendCommand(UserOrgMembershipStateMachine.Command.Deactivate);

    internal StateMachineTransitionResult<UserOrgMembershipStatus> TryReactivate() =>
        _statusManager.TrySendCommand(UserOrgMembershipStateMachine.Command.Reactivate);

    internal bool IsAuthorised()
    {
        UserOrgMembershipStatus[] authorisedStatuses =
        [
            UserOrgMembershipStatus.Active,
            UserOrgMembershipStatus.Inactive,
        ];
        return authorisedStatuses.Contains(Status);
    }

    public static IEnumerable<UserMembershipAction> GetPermittedActions(
        UserOrgMembershipStatus status
    )
    {
        UserMembershipAction[] nonCommandRelatedActions =
            status == UserOrgMembershipStatus.Active || status == UserOrgMembershipStatus.Inactive
                ? [UserMembershipAction.EditUserRole]
                : [];
        var statusManager = new UserOrgMembershipStateMachine(status);
        IEnumerable<UserOrgMembershipStateMachine.Command> permittedCommands =
            statusManager.GetPermittedStateChangingCommands();
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
