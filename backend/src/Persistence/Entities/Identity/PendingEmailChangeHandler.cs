using UKPS.Api.Application.Common;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed record PendingEmailChangeHandler : PendingEmailChange
{
    private string? _workEmail;
    private string? _verificationToken;
    private DateTime? _verifiedAt;

    public PendingEmailChangeHandler(string workEmail, string verificationToken)
    {
        _workEmail = workEmail;
        _verificationToken = verificationToken;
    }

    private PendingEmailChangeHandler() { }

    internal override Result<Data, PendingEmailError> TryGetData()
    {
        if (_workEmail is null)
        {
            return Result<Data, PendingEmailError>.Err(new PendingEmailError.NoPendingEmail());
        }

        return Result<Data, PendingEmailError>.Ok(new Data(_workEmail, _verifiedAt is not null));
    }

    internal Result<string, RefreshVerificationTokenError> TryRefreshVerificationToken(
        string verificationToken
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

    internal Result<PendingEmailCancellationError> TryCancelPendingEmailChange()
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

    /// <summary>Creates a handler with no change requested.</summary>
    public static PendingEmailChangeHandler Empty()
    {
        return new PendingEmailChangeHandler();
    }
}
