using System.Diagnostics;

namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents an error that can occur when resending a setup token.
/// </summary>
public abstract record ResendSetupTokenError
{
    /// <summary>
    /// Indicates that the setup token could not be found.
    /// </summary>
    public sealed record DoesNotExist : ResendSetupTokenError;

    /// <summary>
    /// Indicates that the setup token has already been used and can no longer be reissued.
    /// </summary>
    public sealed record Consumed : ResendSetupTokenError;

    /// <summary>
    /// Indicates that the setup token has already been resent the maximum number of times.
    /// </summary>
    public sealed record TooManyAttempts : ResendSetupTokenError;

    internal TResult Match<TResult>(
        Func<DoesNotExist, TResult> doesNotExist,
        Func<Consumed, TResult> consumed,
        Func<TooManyAttempts, TResult> tooManyAttempts
    )
    {
        return this switch
        {
            DoesNotExist x => doesNotExist(x),
            Consumed x => consumed(x),
            TooManyAttempts x => tooManyAttempts(x),
            _ => throw new UnreachableException("Unknown resend setup token error."),
        };
    }
}
