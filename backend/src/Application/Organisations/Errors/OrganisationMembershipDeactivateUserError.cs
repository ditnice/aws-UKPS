using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Organisations.Errors;

/// <summary>
/// Represents an error that can occur when attempting to deactivate a user
/// in an organisation membership context.
/// </summary>
public abstract record OrganisationMembershipDeactivateUserError
{
    /// <summary>
    /// Represents an error indicating that the operation is not allowed for
    /// the specified organisation.
    /// </summary>
    /// <param name="OrganisationId">
    /// The identifier of the organisation where the operation is not allowed.
    /// </param>
    internal sealed record NotAllowed(int OrganisationId)
        : OrganisationMembershipDeactivateUserError;

    /// <summary>
    /// Represents an error indicating that the specified organisation or user
    /// could not be found.
    /// </summary>
    public sealed record NotFound() : OrganisationMembershipDeactivateUserError;

    /// <summary>
    /// Represents an error indicating that the user cannot be deactivated
    /// because the organisation membership is in its current state.
    /// </summary>
    /// <param name="TransitionResult">
    /// The result of the attempted transition.
    /// </param>
    public sealed record NotAllowedInCurrentState(
        StateMachineTransitionResult<UserOrgStatus> TransitionResult
    ) : OrganisationMembershipDeactivateUserError;

    internal TResult Match<TResult>(
        Func<NotAllowed, TResult> notAllowed,
        Func<TResult> notFound,
        Func<NotAllowedInCurrentState, TResult> notAllowedInCurrentState
    ) =>
        this switch
        {
            NotAllowed error => notAllowed(error),
            NotFound => notFound(),
            NotAllowedInCurrentState error => notAllowedInCurrentState(error),
            _ => throw new InvalidOperationException($"Unknown error type: {GetType().Name}"),
        };
}
