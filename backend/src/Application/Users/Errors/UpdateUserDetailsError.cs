using System.Diagnostics;

namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when updating a user's details.
/// </summary>
public abstract record UpdateUserDetailsError
{
    /// <summary>
    /// Indicates that the current user is not authorised to update the user's details.
    /// </summary>
    public sealed record Unauthorised : UpdateUserDetailsError;

    /// <summary>
    /// Indicates that the user to be updated does not exist.
    /// </summary>
    public sealed record UserDoesNotExist : UpdateUserDetailsError;

    /// <summary>
    /// Indicates that the email address conflicts with an existing user's email address.
    /// </summary>
    internal sealed record ConflictingEmail : UpdateUserDetailsError;

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
