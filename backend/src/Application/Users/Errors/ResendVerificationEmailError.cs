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
    /// The entity's pending work email has already been verified,
    /// so there is no need to resend a verification email.
    /// </summary>
    public sealed record PendingEmailAlreadyVerified : ResendVerificationEmailError;

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

    /// <summary>
    /// Exhaustively maps this error to a <typeparamref name="TResult"/> by invoking
    /// the handler that corresponds to its concrete type.
    /// </summary>
    /// <typeparam name="TResult">The type returned by each handler.</typeparam>
    /// <param name="notFound">Invoked when the error is <see cref="NotFound"/>.</param>
    /// <param name="noPendingVerification">Invoked when the error is <see cref="NoPendingVerification"/>.</param>
    /// <param name="pendingEmailAlreadyVerified">Invoked when the error is <see cref="PendingEmailAlreadyVerified"/>.</param>
    /// <param name="forbidden">Invoked when the error is <see cref="Forbidden"/>.</param>
    /// <param name="concurrencyDetected">Invoked when the error is <see cref="ConcurrencyDetected"/>.</param>
    /// <returns>The result produced by the handler matching the error's concrete type.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the error is of a type not covered by any handler.
    /// </exception>
    internal TResult Match<TResult>(
        Func<NotFound, TResult> notFound,
        Func<NoPendingVerification, TResult> noPendingVerification,
        Func<PendingEmailAlreadyVerified, TResult> pendingEmailAlreadyVerified,
        Func<Forbidden, TResult> forbidden,
        Func<ConcurrencyDetected, TResult> concurrencyDetected
    ) =>
        this switch
        {
            NotFound e => notFound(e),
            NoPendingVerification e => noPendingVerification(e),
            PendingEmailAlreadyVerified e => pendingEmailAlreadyVerified(e),
            Forbidden e => forbidden(e),
            ConcurrencyDetected e => concurrencyDetected(e),
            _ => throw new InvalidOperationException(
                $"Unknown {nameof(ResendVerificationEmailError)} type: {GetType().Name}."
            ),
        };
}
