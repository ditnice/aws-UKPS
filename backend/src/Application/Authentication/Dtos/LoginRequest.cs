using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the credentials provided by a user when attempting to authenticate.
/// </summary>
public sealed record LoginRequest
{
    /// <summary>
    /// Gets the username used to authenticate with the identity provider.
    /// </summary>
    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Username cannot be empty or whitespace.")]
    public required string Username { get; init; }

    /// <summary>
    /// Gets the password used to authenticate with the identity provider.
    /// </summary>
    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Password cannot be empty or whitespace.")]
    public required string Password { get; init; }
}
