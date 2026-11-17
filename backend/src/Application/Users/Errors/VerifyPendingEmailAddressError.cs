using System.Diagnostics;

namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents the ways in which verifying a pending email address can fail.
/// </summary>
/// <remarks>
/// This is a closed set of error cases. Use <see cref="Match{TResult}"/> to handle each case exhaustively.
/// </remarks>
public abstract record VerifyPendingEmailAddressError
{
    /// <summary>
    /// The user associated with the request could not be found.
    /// </summary>
    internal sealed record UserNotFound : VerifyPendingEmailAddressError;

    /// <summary>
    /// The caller is not authorised to verify the pending email address.
    /// </summary>
    internal sealed record Unauthorised : VerifyPendingEmailAddressError;

    /// <summary>
    /// The user has no pending email address awaiting verification.
    /// </summary>
    internal sealed record NoPendingEmailToVerify : VerifyPendingEmailAddressError;

    /// <summary>
    /// The pending email address has already been verified.
    /// </summary>
    internal sealed record EmailAlreadyVerified : VerifyPendingEmailAddressError;

    /// <summary>
    /// The supplied verification token is invalid.
    /// </summary>
    internal sealed record InvalidTokenProvided : VerifyPendingEmailAddressError;

    /// <summary>
    /// Maps this error to a <typeparamref name="TResult"/> by invoking the delegate that corresponds to its case.
    /// </summary>
    /// <typeparam name="TResult">The type returned by each of the delegates.</typeparam>
    /// <param name="userNotFound">Invoked when the user could not be found.</param>
    /// <param name="unauthorised">Invoked when the caller is not authorised.</param>
    /// <param name="noPendingEmailToVerified">Invoked when there is no pending email address to verify.</param>
    /// <param name="emailAlreadyVerified">Invoked when the email address has already been verified.</param>
    /// <param name="invalidTokenProvided">Invoked when the verification token is invalid.</param>
    /// <returns>The result of the delegate matching this error case.</returns>
    /// <exception cref="UnreachableException">
    /// Thrown if this instance is not one of the recognised error cases.
    /// </exception>
    internal TResult Match<TResult>(
        Func<TResult> userNotFound,
        Func<TResult> unauthorised,
        Func<TResult> noPendingEmailToVerified,
        Func<TResult> emailAlreadyVerified,
        Func<TResult> invalidTokenProvided
    )
    {
        return this switch
        {
            UserNotFound => userNotFound(),
            Unauthorised => unauthorised(),
            NoPendingEmailToVerify => noPendingEmailToVerified(),
            EmailAlreadyVerified => emailAlreadyVerified(),
            InvalidTokenProvided => invalidTokenProvided(),
            _ => throw new UnreachableException(
                $"Unrecognised {nameof(VerifyPendingEmailAddressError)}"
            ),
        };
    }
}
