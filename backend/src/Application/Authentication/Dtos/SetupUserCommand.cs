namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the command used to complete user setup by validating a setup token
/// and assigning a new password to the user.
/// </summary>
public record SetupUserCommand
{
    /// <summary>
    /// Gets the unique token used to identify and validate the pending user setup request.
    /// </summary>
    public required Guid SetupToken { get; init; }

    /// <summary>
    /// Gets the new password to assign to the user account.
    /// </summary>
    public required string NewPassword { get; init; }
}
