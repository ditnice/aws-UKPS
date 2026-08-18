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
    /// Represents an error indicating that the organisation could not be found.
    /// </summary>
    /// <param name="OrganisationName">
    /// The name of the organisation that could not be found.
    /// </param>
    internal sealed record NotFound(string OrganisationName) : RegisterUserError;

    /// <summary>
    /// Represents an error indicating that the email address is already in use.
    /// </summary>
    public sealed record EmailConflict() : RegisterUserError;

    /// <summary>
    /// Represents an error indicating that one or more required fields are missing.
    /// </summary>
    public sealed record MissingFields() : RegisterUserError;
}
