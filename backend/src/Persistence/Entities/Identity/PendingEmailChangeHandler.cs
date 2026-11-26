using UKPS.Api.Application.Common;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed record PendingEmailChangeHandler : PendingEmailChange
{
    private string? _workEmail;
    private string? _verificationToken;
    private DateTime? _verifiedAt;
    private DateTime? _requestedAt;

    public PendingEmailChangeHandler(
        string workEmail,
        string verificationToken,
        DateTime requestedAt
    )
    {
        _workEmail = workEmail;
        _verificationToken = verificationToken;
        _requestedAt = requestedAt;
    }

    private PendingEmailChangeHandler() { }

    internal override Result<Data, PendingEmailError> TryGetData(
        DateTime currentTime,
        TimeSpan timeout
    )
    {
        if (_workEmail is null)
        {
            return Result<Data, PendingEmailError>.Err(new PendingEmailError.NoPendingEmail());
        }
        if (HasTimedOut(currentTime, timeout))
        {
            return Result<Data, PendingEmailError>.Err(
                new PendingEmailError.PendingEmailTimedOut()
            );
        }

        return Result<Data, PendingEmailError>.Ok(new Data(_workEmail, _verifiedAt is not null));
    }

    internal Result<string, RefreshVerificationTokenError> TryRefreshVerificationToken(
        string verificationToken,
        DateTime dateTime,
        TimeSpan timeout
    )
    {
        var dataResult = TryGetData(dateTime, timeout);
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
        DateTime dateTime,
        TimeSpan timeout
    )
    {
        var dataResult = TryGetData(dateTime, timeout);
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

    internal Result<PendingEmailCancellationError> TryCancelPendingEmailChange(
        DateTime currentTime,
        TimeSpan timeout
    )
    {
        if (TryGetData(currentTime, timeout).IsErr)
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

    internal Result<string, CompletionError> TryComplete(DateTime dateTime, TimeSpan timeout)
    {
        var dataResult = TryGetData(dateTime, timeout);
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

    private bool HasTimedOut(DateTime currentTime, TimeSpan timeout) =>
        _requestedAt is not { } requestedAt || currentTime - requestedAt >= timeout;

    private void Clear()
    {
        _workEmail = null;
        _verificationToken = null;
        _verifiedAt = null;
        _requestedAt = null;
    }

    /// <summary>Creates a handler with no change requested.</summary>
    public static PendingEmailChangeHandler Empty()
    {
        return new PendingEmailChangeHandler();
    }
}
