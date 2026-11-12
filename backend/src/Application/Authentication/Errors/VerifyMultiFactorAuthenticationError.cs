using System.Diagnostics;

namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents an error that can occur while verifying a user's multi-factor authentication code.
/// </summary>
public abstract record VerifyMultiFactorAuthenticationError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyMultiFactorAuthenticationError"/> class.
    /// </summary>
    protected VerifyMultiFactorAuthenticationError() { }

    /// <summary>
    /// Indicates that the supplied multi-factor authentication code is invalid.
    /// </summary>
    public sealed record InvalidCode : VerifyMultiFactorAuthenticationError;

    /// <summary>
    /// Indicates that the multi-factor authentication onboarding record does not exist.
    /// </summary>
    public sealed record DoesNotExist : VerifyMultiFactorAuthenticationError;

    /// <summary>
    /// Indicates that setup has already been completed for this user.
    /// </summary>
    public sealed record AlreadyCompleted : VerifyMultiFactorAuthenticationError;

    /// <summary>
    /// Indicates that the setup token or session has expired.
    /// </summary>
    public sealed record Expired : VerifyMultiFactorAuthenticationError;

    /// <summary>
    /// Indicates that the authentication session is no longer valid.
    /// </summary>
    public sealed record SessionExpired : VerifyMultiFactorAuthenticationError;

    /// <summary>
    /// Indicates that a rate limit was exceeded.
    /// </summary>
    public sealed record RateLimited : VerifyMultiFactorAuthenticationError;

    /// <summary>
    /// Executes the delegate corresponding to the concrete error type.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the delegate.</typeparam>
    /// <param name="invalidCode">
    /// The delegate to execute when the error is an <see cref="InvalidCode"/>.
    /// </param>
    /// <param name="doesNotExist">
    /// The delegate to execute when the error is a <see cref="DoesNotExist"/>.
    /// </param>
    /// <param name="alreadyCompleted">
    /// The delegate to execute when the error is an <see cref="AlreadyCompleted"/>.
    /// </param>
    /// <param name="expired">
    /// The delegate to execute when the error is an <see cref="Expired"/>.
    /// </param>
    /// <param name="sessionExpired">
    /// The delegate to execute when the error is a <see cref="SessionExpired"/>.
    /// </param>
    /// <param name="rateLimited">
    /// The delegate to execute when the error is a <see cref="RateLimited"/>.
    /// </param>
    /// <returns>The value returned by the matching delegate.</returns>
    internal TResult Match<TResult>(
        Func<InvalidCode, TResult> invalidCode,
        Func<DoesNotExist, TResult> doesNotExist,
        Func<AlreadyCompleted, TResult> alreadyCompleted,
        Func<Expired, TResult> expired,
        Func<SessionExpired, TResult> sessionExpired,
        Func<RateLimited, TResult> rateLimited
    ) =>
        this switch
        {
            InvalidCode x => invalidCode(x),
            DoesNotExist x => doesNotExist(x),
            AlreadyCompleted x => alreadyCompleted(x),
            Expired x => expired(x),
            SessionExpired x => sessionExpired(x),
            RateLimited x => rateLimited(x),
            _ => throw new UnreachableException(
                $"Unhandled {nameof(VerifyMultiFactorAuthenticationError)} type: {GetType().Name}."
            ),
        };
}
