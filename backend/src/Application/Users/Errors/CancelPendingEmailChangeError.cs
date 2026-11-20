using System.Diagnostics;

namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents the ways in which cancelling a pending email change can fail.
/// </summary>
public abstract record CancelPendingEmailChangeError
{
    /// <summary>
    /// Indicates that the current user is not authorised to cancel the pending email change.
    /// </summary>
    internal sealed record Unauthorised : CancelPendingEmailChangeError;

    /// <summary>
    /// Indicates that the user does not exist.
    /// </summary>
    internal sealed record UserDoesNotExist : CancelPendingEmailChangeError;

    /// <summary>
    /// Indicates that the user has no pending email address to cancel.
    /// </summary>
    internal sealed record NoPendingEmailToCancel : CancelPendingEmailChangeError;

    /// <summary>
    /// Indicates that the pending email address has already been verified and cannot be cancelled.
    /// </summary>
    internal sealed record EmailAlreadyVerified : CancelPendingEmailChangeError;

    /// <summary>
    /// Indicates that the user was modified by another operation while the cancellation was in progress.
    /// </summary>
    internal sealed record ConcurrentChangeDetected : CancelPendingEmailChangeError;

    internal TResult Match<TResult>(
        Func<TResult> unauthorised,
        Func<TResult> userDoesNotExist,
        Func<TResult> noPendingEmailToCancel,
        Func<TResult> emailAlreadyVerified,
        Func<TResult> concurrentChangeDetected
    )
    {
        return this switch
        {
            Unauthorised => unauthorised(),
            UserDoesNotExist => userDoesNotExist(),
            NoPendingEmailToCancel => noPendingEmailToCancel(),
            EmailAlreadyVerified => emailAlreadyVerified(),
            ConcurrentChangeDetected => concurrentChangeDetected(),
            _ => throw new UnreachableException(
                $"Unrecognised {nameof(CancelPendingEmailChangeError)}"
            ),
        };
    }
}
