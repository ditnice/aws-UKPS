using System.Diagnostics;
using UKPS.Api.Application.Common;

namespace UKPS.Api.Persistence.Entities.Identity;

/// <summary>
/// A requested change to a user's work email that has not yet been completed.
/// Mapped as an optional complex property on <see cref="User"/>, stored in the <c>app_user</c> table.
/// </summary>
internal sealed record PendingEmailChange
{
    private string? _workEmail;
    private string? _verificationToken;
    private DateTime? _verifiedAt;

    public PendingEmailChange(string workEmail, string verificationToken)
    {
        _workEmail = workEmail;
        _verificationToken = verificationToken;
    }

    private PendingEmailChange() { }

    internal Result<Data, PendingEmailError> TryGetData()
    {
        if (_workEmail is null)
        {
            return Result<Data, PendingEmailError>.Err(new PendingEmailError.NoPendingEmail());
        }

        return Result<Data, PendingEmailError>.Ok(new Data(_workEmail, _verifiedAt is not null));
    }

    internal Result<string, RefreshVerificationTokenError> TryRefreshVerificationToken(
        string verificationToken,
        DateTime dateTime
    )
    {
        if (!TryGetData().TryGetSuccessValue(out var data))
        {
            return Result<string, RefreshVerificationTokenError>.Err(
                new RefreshVerificationTokenError.NoPendingEmailToRefresh()
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
        if (!TryGetData().TryGetSuccessValue(out var data))
        {
            return Result<VerificationError>.Err(new VerificationError.NoPendingEmailToVerify());
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
        if (TryGetData().IsErr)
        {
            return Result<PendingEmailCancellationError>.Err(
                new PendingEmailCancellationError.NoPendingEmailToCancel()
            );
        }

        _workEmail = null;
        _verificationToken = null;
        _verifiedAt = null;

        return Result<PendingEmailCancellationError>.Ok();
    }

    internal Result<string, CompletionError> TryComplete()
    {
        if (!TryGetData().TryGetSuccessValue(out var data))
        {
            return Result<string, CompletionError>.Err(
                new CompletionError.NoPendingEmailToComplete()
            );
        }
        if (!data.Verified)
        {
            return Result<string, CompletionError>.Err(new CompletionError.NotVerified());
        }

        _workEmail = null;
        _verificationToken = null;
        _verifiedAt = null;

        return Result<string, CompletionError>.Ok(data.WorkEmail);
    }

    public static PendingEmailChange Empty()
    {
        return new PendingEmailChange();
    }

    public record Data(string WorkEmail, bool Verified);

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

        public sealed record NoPendingEmail : PendingEmailError;
    }

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

        public sealed record NoPendingEmailToVerify : VerificationError;

        public sealed record InvalidVerificationToken : VerificationError;

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

        public sealed record NoPendingEmailToRefresh : RefreshVerificationTokenError;

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

        public sealed record NoPendingEmailToComplete : CompletionError;

        public sealed record NotVerified : CompletionError;
    }
}
