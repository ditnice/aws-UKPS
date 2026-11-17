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

    internal TResult Match<TResult>(
        Func<TResult> unauthorised,
        Func<TResult> userDoesNotExist,
        Func<TResult> conflictingEmail
    )
    {
        return this switch
        {
            Unauthorised => unauthorised(),
            UserDoesNotExist => userDoesNotExist(),
            ConflictingEmail => conflictingEmail(),
            _ => throw new UnreachableException($"Unrecognised {nameof(UpdateUserDetailsError)}"),
        };
    }
}
