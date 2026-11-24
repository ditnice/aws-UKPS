using System.Diagnostics;
using UKPS.Api.Application.Common;

namespace UKPS.Api.Persistence.Entities.Identity;

/// <summary>
/// A requested change to a user's work email that has not yet been completed.
/// Mapped as an optional complex property on <see cref="User"/>, stored in the <c>app_user</c> table.
/// A pending change expires <see cref="Timeout"/> after it was requested; once expired every
/// operation fails with a timed-out error.
/// </summary>
internal sealed record PendingEmailChange
{
    /// <summary>
    /// How long after being requested a pending email change remains usable.
    /// </summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(120);

    private string? _workEmail;
    private string? _verificationToken;
    private DateTime? _verifiedAt;
    private DateTime? _requestedAt;

    public PendingEmailChange(string workEmail, string verificationToken, DateTime requestedAt)
    {
        _workEmail = workEmail;
        _verificationToken = verificationToken;
        _requestedAt = requestedAt;
    }

    private PendingEmailChange() { }

    internal Result<Data, PendingEmailError> TryGetData(DateTime currentTime)
    {
        if (_workEmail is null)
        {
            return Result<Data, PendingEmailError>.Err(new PendingEmailError.NoPendingEmail());
        }
        if (HasTimedOut(currentTime))
        {
            return Result<Data, PendingEmailError>.Err(
                new PendingEmailError.PendingEmailTimedOut()
            );
        }

        return Result<Data, PendingEmailError>.Ok(new Data(_workEmail, _verifiedAt is not null));
    }

    internal Result<string, RefreshVerificationTokenError> TryRefreshVerificationToken(
        string verificationToken,
        DateTime dateTime
    )
    {
        var dataResult = TryGetData(dateTime);
        if (!dataResult.TryGetSuccessValue(out var data))
        {
            return Result<string, RefreshVerificationTokenError>.Err(
                dataResult.Error!.Match<RefreshVerificationTokenError>(
                    noPendingEmail: _ => new RefreshVerificationTokenError.NoPendingEmailToRefresh(),
                    pendingEmailTimedOut: _ => new RefreshVerificationTokenError.PendingEmailTimedOut()
                )
            );
        }
        if (data.Verified)
        {
            return Result<string, RefreshVerificationTokenError>.Err(
                new RefreshVerificationTokenError.AlreadyVerified()
            );
        }

        _verificationToken = verificationToken;

        return Result<string, RefreshVerificationTokenError>.Ok(data.WorkEmail);
    }

    internal Result<VerificationError> TryMarkPendingEmailAsVerified(
        string verificationToken,
        DateTime dateTime
    )
    {
        var dataResult = TryGetData(dateTime);
        if (!dataResult.TryGetSuccessValue(out var data))
        {
            return Result<VerificationError>.Err(
                dataResult.Error!.Match<VerificationError>(
                    noPendingEmail: _ => new VerificationError.NoPendingEmailToVerify(),
                    pendingEmailTimedOut: _ => new VerificationError.PendingEmailTimedOut()
                )
            );
        }
        if (data.Verified)
        {
            return Result<VerificationError>.Err(new VerificationError.AlreadyVerified());
        }
        var verificationTokensMatch = string.Equals(
            verificationToken,
            _verificationToken,
            StringComparison.Ordinal
        );
        if (!verificationTokensMatch)
        {
            return Result<VerificationError>.Err(new VerificationError.InvalidVerificationToken());
        }

        _verifiedAt = dateTime;
        _verificationToken = null;

        return Result<VerificationError>.Ok();
    }

    internal Result<PendingEmailCancellationError> TryCancelPendingEmailChange(DateTime dateTime)
    {
        var dataResult = TryGetData(dateTime);
        if (!dataResult.TryGetSuccessValue(out _))
        {
            return Result<PendingEmailCancellationError>.Err(
                dataResult.Error!.Match<PendingEmailCancellationError>(
                    noPendingEmail: _ => new PendingEmailCancellationError.NoPendingEmailToCancel(),
                    pendingEmailTimedOut: _ => new PendingEmailCancellationError.PendingEmailTimedOut()
                )
            );
        }

        Clear();

        return Result<PendingEmailCancellationError>.Ok();
    }

    internal Result<string, CompletionError> TryComplete(DateTime dateTime)
    {
        var dataResult = TryGetData(dateTime);
        if (!dataResult.TryGetSuccessValue(out var data))
        {
            return Result<string, CompletionError>.Err(
                dataResult.Error!.Match<CompletionError>(
                    noPendingEmail: _ => new CompletionError.NoPendingEmailToComplete(),
                    pendingEmailTimedOut: _ => new CompletionError.PendingEmailTimedOut()
                )
            );
        }
        if (!data.Verified)
        {
            return Result<string, CompletionError>.Err(new CompletionError.NotVerified());
        }

        Clear();

        return Result<string, CompletionError>.Ok(data.WorkEmail);
    }

    public static PendingEmailChange Empty()
    {
        return new PendingEmailChange();
    }

    // A pending change with no request time predates the timeout and is treated as expired.
    private bool HasTimedOut(DateTime currentTime) =>
        _requestedAt is not { } requestedAt || currentTime - requestedAt >= Timeout;

    private void Clear()
    {
        _workEmail = null;
        _verificationToken = null;
        _verifiedAt = null;
        _requestedAt = null;
    }

    public record Data(string WorkEmail, bool Verified);

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
}
