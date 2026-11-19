namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when completing a pending email update.
/// </summary>
public abstract record UpdateEmailToCompletePendingEmailError
{
    /// <summary>
    /// Represents the error when the pending email update cannot be found.
    /// </summary>
    public sealed record NotFound : UpdateEmailToCompletePendingEmailError;

    /// <summary>
    /// Represents the error when the pending email update has not been verified.
    /// </summary>
    public sealed record NotVerified : UpdateEmailToCompletePendingEmailError;

    /// <summary>
    /// Represents the error when the email address conflicts with an existing user.
    /// </summary>
    public sealed record ConflictingEmail : UpdateEmailToCompletePendingEmailError;

    internal TResult Match<TResult>(
        Func<TResult> notFound,
        Func<TResult> conflictingEmail,
        Func<TResult> notVerified
    )
    {
        ArgumentNullException.ThrowIfNull(notFound);
        ArgumentNullException.ThrowIfNull(conflictingEmail);

        return this switch
        {
            NotFound => notFound(),
            ConflictingEmail => conflictingEmail(),
            NotVerified => notVerified(),
            _ => throw new NotSupportedException(
                $"Unsupported {nameof(UpdateEmailToCompletePendingEmailError)} subtype: {GetType().Name}."
            ),
        };
    }
}
