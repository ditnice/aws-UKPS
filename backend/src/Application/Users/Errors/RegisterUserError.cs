namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when registering a new user.
/// </summary>
public abstract record RegisterUserError
{
    /// <summary>
    /// Prevents external inheritance of the <see cref="RegisterUserError"/> record.
    /// </summary>
    private protected RegisterUserError() { }

    /// <summary>
    /// Represents an error indicating that one or more required fields are missing.
    /// </summary>
    public sealed record MissingFields() : RegisterUserError;
}
