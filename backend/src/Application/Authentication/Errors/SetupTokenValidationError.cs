using System.Diagnostics;

namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents an error that can occur when validating a setup token.
/// </summary>
public abstract record SetupTokenValidationError
{
    /// <summary>
    /// Indicates that the setup token has expired and is no longer valid.
    /// </summary>
    public sealed record Expired : SetupTokenValidationError;

    /// <summary>
    /// Indicates that the setup token could not be found.
    /// </summary>
    public sealed record DoesNotExist : SetupTokenValidationError;

    /// <summary>
    /// Indicates that the setup token has already been used and can no longer be validated.
    /// </summary>
    public sealed record Consumed : SetupTokenValidationError;

    internal TResult Match<TResult>(
        Func<Expired, TResult> expired,
        Func<DoesNotExist, TResult> doesNotExist,
        Func<Consumed, TResult> consumed
    )
    {
        return this switch
        {
            Expired x => expired(x),
            DoesNotExist x => doesNotExist(x),
            Consumed x => consumed(x),
            _ => throw new UnreachableException("Unknown setup token validation error."),
        };
    }
}
