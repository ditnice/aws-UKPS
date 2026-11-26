using System.Diagnostics;
using System.Linq.Expressions;
using UKPS.Api.Application.Common;

namespace UKPS.Api.Persistence.Entities.Identity;

internal abstract record PendingEmailChange
{
    private protected string? _workEmail;
    private protected string? _verificationToken;
    private protected DateTime? _verifiedAt;
    private protected DateTime? _requestedAt;

    /// <summary>
    /// EF Core translatable filter for users whose pending email change is still active
    /// (requested and not timed out). Use with <c>Where</c> on an <see cref="IQueryable{User}"/>.
    /// The cutoff is computed client-side so the database only does a plain comparison.
    /// </summary>
    internal static Expression<Func<User, bool>> HasActivePendingEmail(
        DateTime currentTime,
        TimeSpan timeout
    )
    {
        DateTime cutoff = currentTime - timeout;
        Expression<Func<PendingEmailChange, bool>> isActive = pendingEmail =>
            pendingEmail._workEmail != null && pendingEmail._requestedAt > cutoff;

        Expression<Func<User, PendingEmailChangeHandler>> accessor =
            User._pendingEmailHandlerAccessor;
        Expression body = new ReplaceParameterVisitor(isActive.Parameters[0], accessor.Body).Visit(
            isActive.Body
        );
        return Expression.Lambda<Func<User, bool>>(body, accessor.Parameters);
    }

    private sealed class ReplaceParameterVisitor(ParameterExpression from, Expression to)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == from ? to : base.VisitParameter(node);
    }

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
