using System.Diagnostics;

namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents the possible errors that can occur when verifying a multi-factor
/// authentication update.
/// </summary>
public abstract record VerifyMultiFactorAuthenticationUpdateError
{
    /// <summary>
    /// Represents an error indicating that the supplied verification code was invalid.
    /// </summary>
    public sealed record InvalidCode : VerifyMultiFactorAuthenticationUpdateError;

    /// <summary>
    /// Maps this error to a result by invoking the handler that corresponds to its concrete type.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the handler.</typeparam>
    /// <param name="invalidCode">
    /// The function to invoke when the error is an <see cref="InvalidCode"/>.
    /// </param>
    /// <returns>The value produced by the handler that matches this error's type.</returns>
    /// <exception cref="UnreachableException">
    /// Thrown when the error is of a type that is not handled by this method.
    /// </exception>
    internal TResult Match<TResult>(Func<InvalidCode, TResult> invalidCode) =>
        this switch
        {
            InvalidCode x => invalidCode(x),
            _ => throw new UnreachableException(
                $"Unhandled {nameof(VerifyMultiFactorAuthenticationUpdateError)} type: {GetType().Name}."
            ),
        };
}
