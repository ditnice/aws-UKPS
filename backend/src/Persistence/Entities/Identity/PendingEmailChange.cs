using System.Diagnostics;
using UKPS.Api.Application.Common;

namespace UKPS.Api.Persistence.Entities.Identity;

internal abstract record PendingEmailChange
{
    internal abstract Result<Data, PendingEmailError> TryGetData(
        DateTime currentTime,
        TimeSpan timeout
    );

    /// <summary>Why <see cref="TryGetData"/> could not return the change.</summary>
    public abstract record PendingEmailError
    {
        protected PendingEmailError() { }

        public TResult Match<TResult>(
            Func<NoPendingEmail, TResult> noPendingEmail,
            Func<PendingEmailTimedOut, TResult> pendingEmailTimedOut
        ) =>
            this switch
            {
                NoPendingEmail e => noPendingEmail(e),
                PendingEmailTimedOut e => pendingEmailTimedOut(e),
                _ => throw new UnreachableException(
                    $"Unhandled {nameof(PendingEmailError)}: {GetType().Name}"
                ),
            };

        public sealed record NoPendingEmail : PendingEmailError;

        public sealed record PendingEmailTimedOut : PendingEmailError;
    }

    /// <summary>Why the requested work email could not be marked as verified.</summary>
    public abstract record VerificationError
    {
        protected VerificationError() { }

        public TResult Match<TResult>(
            Func<NoPendingEmailToVerify, TResult> noPendingEmailToVerify,
            Func<InvalidVerificationToken, TResult> invalidVerificationToken,
            Func<AlreadyVerified, TResult> alreadyVerified,
            Func<PendingEmailTimedOut, TResult> pendingEmailTimedOut
        ) =>
            this switch
            {
                NoPendingEmailToVerify e => noPendingEmailToVerify(e),
                InvalidVerificationToken e => invalidVerificationToken(e),
                AlreadyVerified e => alreadyVerified(e),
                PendingEmailTimedOut e => pendingEmailTimedOut(e),
                _ => throw new UnreachableException(
                    $"Unhandled {nameof(VerificationError)}: {GetType().Name}"
                ),
            };

        public sealed record NoPendingEmailToVerify : VerificationError;

        public sealed record InvalidVerificationToken : VerificationError;

        public sealed record AlreadyVerified : VerificationError;

        public sealed record PendingEmailTimedOut : VerificationError;
    }

    internal abstract record PendingEmailCancellationError
    {
        protected PendingEmailCancellationError() { }

        public TResult Match<TResult>(
            Func<NoPendingEmailToCancel, TResult> noPendingEmailToCancel,
            Func<PendingEmailTimedOut, TResult> pendingEmailTimedOut
        ) =>
            this switch
            {
                NoPendingEmailToCancel e => noPendingEmailToCancel(e),
                PendingEmailTimedOut e => pendingEmailTimedOut(e),
                _ => throw new UnreachableException(
                    $"Unhandled {nameof(PendingEmailCancellationError)}: {GetType().Name}"
                ),
            };

        public sealed record NoPendingEmailToCancel : PendingEmailCancellationError;

        public sealed record PendingEmailTimedOut : PendingEmailCancellationError;
    }

    internal abstract record RefreshVerificationTokenError
    {
        protected RefreshVerificationTokenError() { }

        public TResult Match<TResult>(
            Func<NoPendingEmailToRefresh, TResult> noPendingEmailToRefresh,
            Func<AlreadyVerified, TResult> alreadyVerified,
            Func<PendingEmailTimedOut, TResult> pendingEmailTimedOut
        ) =>
            this switch
            {
                NoPendingEmailToRefresh e => noPendingEmailToRefresh(e),
                AlreadyVerified e => alreadyVerified(e),
                PendingEmailTimedOut e => pendingEmailTimedOut(e),
                _ => throw new UnreachableException(
                    $"Unhandled {nameof(RefreshVerificationTokenError)}: {GetType().Name}"
                ),
            };

        public sealed record NoPendingEmailToRefresh : RefreshVerificationTokenError;

        public sealed record AlreadyVerified : RefreshVerificationTokenError;

        public sealed record PendingEmailTimedOut : RefreshVerificationTokenError;
    }

    internal abstract record CompletionError
    {
        protected CompletionError() { }

        public TResult Match<TResult>(
            Func<NoPendingEmailToComplete, TResult> noPendingEmailToComplete,
            Func<NotVerified, TResult> notVerified,
            Func<PendingEmailTimedOut, TResult> pendingEmailTimedOut
        ) =>
            this switch
            {
                NoPendingEmailToComplete e => noPendingEmailToComplete(e),
                NotVerified e => notVerified(e),
                PendingEmailTimedOut e => pendingEmailTimedOut(e),
                _ => throw new UnreachableException(
                    $"Unhandled {nameof(CompletionError)}: {GetType().Name}"
                ),
            };

        public sealed record NoPendingEmailToComplete : CompletionError;

        public sealed record NotVerified : CompletionError;

        public sealed record PendingEmailTimedOut : CompletionError;
    }

    public record Data(string WorkEmail, bool Verified);
}
