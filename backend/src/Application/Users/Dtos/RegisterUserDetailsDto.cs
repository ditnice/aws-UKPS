using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the details of a user who has been registered.
/// </summary>
public sealed record RegisterUserDetailsDto
{
    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    [Required]
    public required string FullName { get; init; }

    /// <summary>
    /// Gets the user's work email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string WorkEmail { get; init; }

    /// <summary>
    /// Gets the user phone number.
    /// </summary>
    [Required]
    public required string? PhoneNumber { get; init; }
}
