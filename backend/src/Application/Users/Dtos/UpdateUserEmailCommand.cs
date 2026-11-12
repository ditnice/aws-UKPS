using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the command to update the email address of an existing user.
/// </summary>
public sealed record UpdateUserEmailCommand
{
    /// <summary>
    /// Gets the new email address for the user.
    /// </summary>
    /// <remarks>
    /// Must be a valid email address and cannot exceed 255 characters.
    /// </remarks>
    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    [MaxLength(255)]
    public required string Value { get; init; }
}
