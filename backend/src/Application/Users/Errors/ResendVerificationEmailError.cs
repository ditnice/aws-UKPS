namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents the reasons a request to resend a verification email can fail.
/// </summary>
public abstract record ResendVerificationEmailError
{
    /// <summary>
    /// The target entity could not be found.
    /// </summary>
    public sealed record NotFound : ResendVerificationEmailError;

    /// <summary>
    /// The entity exists, but has no pending work email awaiting verification,
    /// so there is nothing to resend.
    /// </summary>
    public sealed record NoPendingVerification : ResendVerificationEmailError;

    /// <summary>
    /// The caller is not permitted to request a verification email resend
    /// for the target entity.
    /// </summary>
    public sealed record Forbidden : ResendVerificationEmailError;

    /// <summary>
    /// The entity was modified by another operation while the resend was being processed.
    /// The caller may retry the operation.
    /// </summary>
    public sealed record ConcurrencyDetected : ResendVerificationEmailError;

    internal TResult Match<TResult>(
        Func<NotFound, TResult> notFound,
        Func<NoPendingVerification, TResult> noPendingVerification,
        Func<Forbidden, TResult> forbidden,
        Func<ConcurrencyDetected, TResult> concurrencyDetected
    ) =>
        this switch
        {
            NotFound e => notFound(e),
            NoPendingVerification e => noPendingVerification(e),
            Forbidden e => forbidden(e),
            ConcurrencyDetected e => concurrencyDetected(e),
            _ => throw new InvalidOperationException(
                $"Unknown {nameof(ResendVerificationEmailError)} type: {GetType().Name}."
            ),
        };
}
