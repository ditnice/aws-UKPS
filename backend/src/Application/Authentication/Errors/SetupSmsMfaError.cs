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

    internal TResult Match<TResult>(
        Func<TResult> invalidPhoneNumber,
        Func<TResult> expired,
        Func<TResult> doesNotExist,
        Func<TResult> unauthorised
    )
    {
        return this switch
        {
            InvalidPhoneNumber => invalidPhoneNumber(),
            Expired => expired(),
            DoesNotExist => doesNotExist(),
            Unauthorised => unauthorised(),
            _ => throw new InvalidOperationException(
                $"Unknown {nameof(SetupSmsMfaError)} type: {GetType().Name}"
            ),
        };
    }
}
