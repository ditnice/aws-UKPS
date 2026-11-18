using System.Diagnostics;

namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when updating a user's email address.
/// </summary>
public abstract record UpdateUserEmailError
{
    /// <summary>
    /// Indicates that the current user is not authorised to update the user's details.
    /// </summary>
    public sealed record Unauthorised : UpdateUserEmailError;

    /// <summary>
    /// Indicates that the user to be updated does not exist.
    /// </summary>
    public sealed record UserDoesNotExist : UpdateUserEmailError;

    /// <summary>
    /// Indicates that the email address being assigned conflicts with an existing user email.
    /// </summary>
    public sealed record ConflictingEmail : UpdateUserEmailError;

    /// <summary>
    /// Indicates that the user was modified by another operation while the update was in progress.
    /// </summary>
    public sealed record ConcurrentChangeDetected : UpdateUserEmailError;

    /// <summary>
    /// Indicates that the email address being assigned is the same as the user's current email address.
    /// </summary>
    public sealed record MatchesCurrentEmail : UpdateUserEmailError;

    internal TResult Match<TResult>(
        Func<TResult> unauthorised,
        Func<TResult> userDoesNotExist,
        Func<TResult> conflictingEmail,
        Func<TResult> matchesCurrentEmail,
        Func<TResult> concurrentChangeDetected
    )
    {
        return this switch
        {
            Unauthorised => unauthorised(),
            UserDoesNotExist => userDoesNotExist(),
            ConflictingEmail => conflictingEmail(),
            MatchesCurrentEmail => matchesCurrentEmail(),
            ConcurrentChangeDetected => concurrentChangeDetected(),
            _ => throw new UnreachableException($"Unrecognised {nameof(UpdateUserEmailError)}"),
        };
    }
}
