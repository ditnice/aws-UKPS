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
    /// Pending email was changed.
    /// </summary>
    internal sealed record ConcurrentChangeDetected : VerifyPendingEmailAddressError;

    internal TResult Match<TResult>(
        Func<TResult> userNotFound,
        Func<TResult> unauthorised,
        Func<TResult> noPendingEmailToVerified,
        Func<TResult> emailAlreadyVerified,
        Func<TResult> invalidTokenProvided,
        Func<TResult> concurrentChangeDetected
    )
    {
        return this switch
        {
            UserNotFound => userNotFound(),
            Unauthorised => unauthorised(),
            NoPendingEmailToVerify => noPendingEmailToVerified(),
            EmailAlreadyVerified => emailAlreadyVerified(),
            InvalidTokenProvided => invalidTokenProvided(),
            ConcurrentChangeDetected => concurrentChangeDetected(),
            _ => throw new UnreachableException(
                $"Unrecognised {nameof(VerifyPendingEmailAddressError)}"
            ),
        };
    }
}
