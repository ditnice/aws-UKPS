namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents an error that occurred while updating a user's password.
/// </summary>
public abstract record UpdatePasswordError
{
    /// <summary>
    /// Prevents direct instantiation of login errors.
    /// </summary>
    protected UpdatePasswordError() { }

    /// <summary>
    /// Represents an error indicating that the supplied credentials were not authorised.
    /// </summary>
    public sealed record Unauthorised : UpdatePasswordError;
}
