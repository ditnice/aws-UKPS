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
    /// Executes the delegate corresponding to the concrete error type.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the delegate.</typeparam>
    /// <param name="invalidCode">
    /// The delegate to execute when the error is an <see cref="InvalidCode"/>.
    /// </param>
    /// <returns>The value returned by the matching delegate.</returns>
    internal TResult Match<TResult>(Func<InvalidCode, TResult> invalidCode) =>
        this switch
        {
            InvalidCode x => invalidCode(x),
            _ => throw new UnreachableException(
                $"Unhandled {nameof(VerifyMultiFactorAuthenticationError)} type: {GetType().Name}."
            ),
        };
}
