using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Authentication;

/// <summary>Confirms a password reset and sets the new password.</summary>
public record ConfirmNewPasswordCommand
{
    /// <summary>Gets the account email address.</summary>
    [Required]
    public required string Email { get; init; }

    /// <summary>Gets the password reset confirmation code.</summary>
    [Required]
    public required string ConfirmationCode { get; init; }

    /// <summary>Gets the new password.</summary>
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [MaxLength(256, ErrorMessage = "Password can be no longer that 256 characters.")]
    public required string NewPassword { get; init; }
}
