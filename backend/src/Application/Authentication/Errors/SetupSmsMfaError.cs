namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents errors that can occur during the user setup process.
/// </summary>
public abstract record SetupSmsMfaError
{
    /// <summary>
    /// Prevents direct instantiation of user setup errors.
    /// </summary>
    protected SetupSmsMfaError() { }

    /// <summary>
    /// Indicates that the supplied phone number does not meet the required validation rules.
    /// </summary>
    public sealed record InvalidPhoneNumber : SetupSmsMfaError;

    /// <summary>
    /// Indicates that the setup token could not be found.
    /// </summary>
    public sealed record DoesNotExist : SetupSmsMfaError;

    /// <summary>
    /// Indicates that the setup token has expired and is no longer valid.
    /// </summary>
    public sealed record Expired : SetupSmsMfaError;

    /// <summary>
    /// Indicates that the user is not authorised to perform this action.
    /// </summary>
    public sealed record Unauthorised : SetupSmsMfaError;

    /// <summary>
    /// Indicates that setup has already been completed for this user.
    /// </summary>
    public sealed record AlreadyCompleted : SetupSmsMfaError;

    /// <summary>
    /// Indicates that too many SMS setup attempts have been made.
    /// </summary>
    public sealed record TooManyAttempts : SetupSmsMfaError;

    /// <summary>
    /// Indicates that SMS setup was attempted too soon after a previous attempt.
    /// </summary>
    public sealed record TooSoon : SetupSmsMfaError;

    /// <summary>
    /// Indicates that a concurrent SMS setup request was detected.
    /// </summary>
    public sealed record ConcurrentRequest : SetupSmsMfaError;

    internal TResult Match<TResult>(
        Func<TResult> invalidPhoneNumber,
        Func<TResult> expired,
        Func<TResult> doesNotExist,
        Func<TResult> unauthorised,
        Func<TResult> alreadyCompleted,
        Func<TResult> tooManyAttempts,
        Func<TResult> tooSoon,
        Func<TResult> concurrentRequest
    )
    {
        return this switch
        {
            InvalidPhoneNumber => invalidPhoneNumber(),
            Expired => expired(),
            DoesNotExist => doesNotExist(),
            Unauthorised => unauthorised(),
            AlreadyCompleted => alreadyCompleted(),
            TooManyAttempts => tooManyAttempts(),
            TooSoon => tooSoon(),
            ConcurrentRequest => concurrentRequest(),
            _ => throw new InvalidOperationException(
                $"Unknown {nameof(SetupSmsMfaError)} type: {GetType().Name}"
            ),
        };
    }
}
