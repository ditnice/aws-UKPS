using System.Diagnostics;

namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents an error that can occur when resending a setup token.
/// </summary>
public abstract record ResendSetupTokenError
{
    /// <summary>
    /// Indicates that the setup token or correlation id (if provided) could not be found.
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

    /// <summary>
    /// Indicates that the request did not supply exactly one of a setup token or a
    /// correlation id.
    /// </summary>
    public sealed record InvalidTokenCombination : ResendSetupTokenError;

    internal TResult Match<TResult>(
        Func<DoesNotExist, TResult> doesNotExist,
        Func<Consumed, TResult> consumed,
        Func<TooManyAttempts, TResult> tooManyAttempts,
        Func<InvalidTokenCombination, TResult> invalidTokenCombination
    )
    {
        return this switch
        {
            DoesNotExist x => doesNotExist(x),
            Consumed x => consumed(x),
            TooManyAttempts x => tooManyAttempts(x),
            InvalidTokenCombination x => invalidTokenCombination(x),
            _ => throw new UnreachableException("Unknown resend setup token error."),
        };
    }
}
