using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the command used to complete a user's password update after an authentication challenge.
/// </summary>
/// <remarks>
/// This command contains the information required to respond to a password update challenge,
/// including the authentication session identifier, the username of the user being updated,
/// and the new password to assign.
/// </remarks>
public sealed record UpdatePasswordCommand
{
    /// <summary>
    /// Gets the authentication session identifier returned by the authentication provider.
    /// </summary>
    /// <remarks>
    /// This value is used to associate the password update request with the existing authentication flow.
    /// </remarks>
    [Required(AllowEmptyStrings = false)]
    public required string AuthenticationSessionId { get; init; }

    /// <summary>
    /// Gets the username of the user whose password is being updated.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string Username { get; init; }

    /// <summary>
    /// Gets the new password to assign to the user.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string NewPassword { get; init; }
}
