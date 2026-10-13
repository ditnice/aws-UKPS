using System.Diagnostics;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when removing a user.
/// </summary>
public abstract record RemoveUserError
{
    /// <summary>
    /// Indicates that the current user is not authorised to remove the specified user.
    /// </summary>
    /// <param name="UserId">The identifier of the user the caller attempted to remove.</param>
    internal sealed record NotAllowed(int UserId) : RemoveUserError;

    /// <summary>
    /// Indicates that the user to be removed does not exist.
    /// </summary>
    /// <param name="UserId">The identifier of the user that was not found.</param>
    public sealed record UserNotFound(int UserId) : RemoveUserError;

    /// <summary>
    /// Indicates that the current user attempted to remove themselves.
    /// </summary>
    /// <param name="UserId">The identifier of the caller's own user.</param>
    public sealed record CannotRemoveSelf(int UserId) : RemoveUserError;

    /// <summary>
    /// Indicates that the user cannot be removed because one of their organisation
    /// memberships is not in a state that allows it (for example, it has already been removed).
    /// </summary>
    /// <param name="TransitionResult">The result of the attempted transition.</param>
    public sealed record NotAllowedInCurrentState(
        StateMachineTransitionResult<UserOrgStatus> TransitionResult
    ) : RemoveUserError;

    internal TResult Match<TResult>(
        Func<TResult> notAllowed,
        Func<TResult> userNotFound,
        Func<TResult> cannotRemoveSelf,
        Func<NotAllowedInCurrentState, TResult> notAllowedInCurrentState
    )
    {
        return this switch
        {
            NotAllowed => notAllowed(),
            UserNotFound => userNotFound(),
            CannotRemoveSelf => cannotRemoveSelf(),
            NotAllowedInCurrentState error => notAllowedInCurrentState(error),
            _ => throw new UnreachableException($"Unrecognised {nameof(RemoveUserError)}"),
        };
    }
}
