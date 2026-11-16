namespace UKPS.Api.Application.InternalServices.Identity;

/// <summary>
/// Represents an error that can occur when creating a new user.
/// </summary>
public abstract record CreateNewUserError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateNewUserError"/> class.
    /// </summary>
    protected CreateNewUserError() { }

    /// <summary>
    /// Indicates that a user with the specified username already exists.
    /// </summary>
    public sealed record UsernameAlreadyExists : CreateNewUserError;
}
