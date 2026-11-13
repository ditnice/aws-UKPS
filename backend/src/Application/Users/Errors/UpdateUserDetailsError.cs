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
    /// Represents a failure to update user details because the supplied data did not pass validation.
    /// </summary>
    /// <param name="ErrorMessage">
    /// A human-readable description of the validation failure, suitable for display or logging.
    /// </param>
    public sealed record ValidationError(string ErrorMessage) : UpdateUserDetailsError;

    internal TResult Match<TResult>(
        Func<TResult> unauthorised,
        Func<TResult> userDoesNotExist,
        Func<ValidationError, TResult> validationError
    )
    {
        return this switch
        {
            Unauthorised => unauthorised(),
            UserDoesNotExist => userDoesNotExist(),
            ValidationError ve => validationError(ve),
            _ => throw new UnreachableException($"Unrecognised {nameof(UpdateUserDetailsError)}"),
        };
    }
}
