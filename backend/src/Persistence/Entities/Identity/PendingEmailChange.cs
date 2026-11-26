using System.Diagnostics;
using UKPS.Api.Application.Common;

namespace UKPS.Api.Persistence.Entities.Identity;

internal abstract record PendingEmailChange
{
    internal abstract Result<Data, PendingEmailError> TryGetData();

    /// <summary>Why <see cref="TryGetData"/> could not return the change.</summary>
    public abstract record PendingEmailError
    {
        protected PendingEmailError() { }

        public TResult Match<TResult>(Func<NoPendingEmail, TResult> noPendingEmail) =>
            this switch
            {
                NoPendingEmail e => noPendingEmail(e),
                _ => throw new UnreachableException(
                    $"Unhandled {nameof(PendingEmailError)}: {GetType().Name}"
                ),
            };

        /// <summary>No work email change has been requested.</summary>
        public sealed record NoPendingEmail : PendingEmailError;
    }

    /// <summary>Why the requested work email could not be marked as verified.</summary>
    public abstract record VerificationError
    {
        protected VerificationError() { }

        public TResult Match<TResult>(
            Func<NoPendingEmailToVerify, TResult> noPendingEmailToVerify,
            Func<InvalidVerificationToken, TResult> invalidVerificationToken,
            Func<AlreadyVerified, TResult> alreadyVerified
        ) =>
            this switch
            {
                NoPendingEmailToVerify e => noPendingEmailToVerify(e),
                InvalidVerificationToken e => invalidVerificationToken(e),
                AlreadyVerified e => alreadyVerified(e),
                _ => throw new UnreachableException(
                    $"Unhandled {nameof(VerificationError)}: {GetType().Name}"
                ),
            };

        /// <summary>No work email change has been requested.</summary>
        public sealed record NoPendingEmailToVerify : VerificationError;

        /// <summary>The token does not match the one issued for the change.</summary>
        public sealed record InvalidVerificationToken : VerificationError;

        /// <summary>The requested work email has already been verified.</summary>
        public sealed record AlreadyVerified : VerificationError;
    }

    internal abstract record PendingEmailCancellationError
    {
        protected PendingEmailCancellationError() { }

        public TResult Match<TResult>(
            Func<NoPendingEmailToCancel, TResult> noPendingEmailToCancel
        ) =>
            this switch
            {
                NoPendingEmailToCancel e => noPendingEmailToCancel(e),
                _ => throw new UnreachableException(
                    $"Unhandled {nameof(PendingEmailCancellationError)}: {GetType().Name}"
                ),
            };

        /// <summary>No work email change has been requested.</summary>
        public sealed record NoPendingEmailToCancel : PendingEmailCancellationError;
    }

    internal abstract record RefreshVerificationTokenError
    {
        protected RefreshVerificationTokenError() { }

        public TResult Match<TResult>(
            Func<NoPendingEmailToRefresh, TResult> noPendingEmailToRefresh,
            Func<AlreadyVerified, TResult> alreadyVerified
        ) =>
            this switch
            {
                NoPendingEmailToRefresh e => noPendingEmailToRefresh(e),
                AlreadyVerified e => alreadyVerified(e),
                _ => throw new UnreachableException(
                    $"Unhandled {nameof(RefreshVerificationTokenError)}: {GetType().Name}"
                ),
            };

        /// <summary>No work email change has been requested.</summary>
        public sealed record NoPendingEmailToRefresh : RefreshVerificationTokenError;

        /// <summary>The requested work email has already been verified, so needs no new token.</summary>
        public sealed record AlreadyVerified : RefreshVerificationTokenError;
    }

    internal abstract record CompletionError
    {
        protected CompletionError() { }

        public TResult Match<TResult>(
            Func<NoPendingEmailToComplete, TResult> noPendingEmailToComplete,
            Func<NotVerified, TResult> notVerified
        ) =>
            this switch
            {
                NoPendingEmailToComplete e => noPendingEmailToComplete(e),
                NotVerified e => notVerified(e),
                _ => throw new UnreachableException(
                    $"Unhandled {nameof(CompletionError)}: {GetType().Name}"
                ),
            };

        /// <summary>No work email change has been requested.</summary>
        public sealed record NoPendingEmailToComplete : CompletionError;

        /// <summary>The requested work email has not been verified yet.</summary>
        public sealed record NotVerified : CompletionError;
    }

    public record Data(string WorkEmail, bool Verified);
}
