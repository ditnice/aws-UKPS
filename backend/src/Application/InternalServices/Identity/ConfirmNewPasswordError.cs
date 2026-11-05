namespace UKPS.Api.Application.InternalServices.Identity;

/// <summary>
/// Represents the ways in which confirming a new password (the second step of a
/// password reset flow) can fail.
/// </summary>
/// <remarks>
/// This is a closed set of cases. Use <see cref="Match{TResult}"/> to handle every
/// case exhaustively.
/// </remarks>
public abstract record ConfirmNewPasswordError
{
    /// <summary>
    /// The confirmation code supplied does not match the one that was issued,
    /// or it has expired.
    /// </summary>
    public sealed record CodeMismatch() : ConfirmNewPasswordError;

    /// <summary>
    /// The confirmation code has expired.
    /// </summary>
    public sealed record CodeExpired : ConfirmNewPasswordError;

    /// <summary>
    /// The new password does not satisfy the password policy
    /// (for example length or required character classes).
    /// </summary>
    public sealed record InvalidPassword() : ConfirmNewPasswordError;

    /// <summary>
    /// Too many requests were made in a short period and the request was throttled.
    /// The caller may retry after a delay.
    /// </summary>
    public sealed record TooManyRequests() : ConfirmNewPasswordError;

    /// <summary>
    /// Too many failed confirmation attempts were made, so further attempts are
    /// blocked. A new confirmation code may need to be requested.
    /// </summary>
    public sealed record TooManyFailedAttempts() : ConfirmNewPasswordError;

    internal TResult Match<TResult>(
        Func<CodeMismatch, TResult> codeMismatch,
        Func<CodeExpired, TResult> codeExpired,
        Func<InvalidPassword, TResult> invalidPassword,
        Func<TooManyRequests, TResult> tooManyRequests,
        Func<TooManyFailedAttempts, TResult> tooManyFailedAttempts
    ) =>
        this switch
        {
            CodeMismatch e => codeMismatch(e),
            CodeExpired e => codeExpired(e),
            InvalidPassword e => invalidPassword(e),
            TooManyRequests e => tooManyRequests(e),
            TooManyFailedAttempts e => tooManyFailedAttempts(e),
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(ConfirmNewPasswordError)} case: {GetType().Name}"
            ),
        };
}
