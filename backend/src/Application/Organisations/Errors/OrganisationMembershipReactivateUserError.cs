using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Organisations.Errors;

/// <summary>
/// Represents an error that can occur when attempting to reactivate a user
/// in an organisation membership context.
/// </summary>
public abstract record OrganisationMembershipReactivateUserError
{
    /// <summary>
    /// Prevents external inheritance of this error type.
    /// </summary>
    protected OrganisationMembershipReactivateUserError() { }

    /// <summary>
    /// Represents an error indicating that the user cannot be reactivated
    /// because their membership is not currently in a state that permits reactivation.
    /// </summary>
    /// <param name="TransitionResult">
    /// The result of evaluating the state machine transition, including the current
    /// state and the states to which the membership can transition.
    /// </param>
    internal sealed record NotAllowedInCurrentState(
        StateMachineTransitionResult<UserOrgStatus> TransitionResult
    ) : OrganisationMembershipReactivateUserError;

    /// <summary>
    /// Represents an error indicating that the current user is not authorised
    /// to reactivate the organisation membership.
    /// </summary>
    internal sealed record NotAllowed : OrganisationMembershipReactivateUserError { }

    /// <summary>
    /// Represents an error indicating that the specified organisation membership
    /// could not be found.
    /// </summary>
    internal sealed record NotFound : OrganisationMembershipReactivateUserError { }

    internal T Match<T>(
        Func<NotAllowed, T> notAllowed,
        Func<T> notFound,
        Func<NotAllowedInCurrentState, T> notAllowedInCurrentState
    )
    {
        return this switch
        {
            NotAllowed na => notAllowed(na),
            NotFound => notFound(),
            NotAllowedInCurrentState error => notAllowedInCurrentState(error),
            _ => throw new InvalidOperationException($"Unknown error type: {GetType().Name}"),
        };
    }
}
