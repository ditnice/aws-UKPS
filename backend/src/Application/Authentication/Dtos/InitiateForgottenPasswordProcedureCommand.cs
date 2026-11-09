using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents a request to start the forgotten password flow for a user.
/// </summary>
public record InitiateForgottenPasswordProcedureCommand
{
    /// <summary>
    /// Gets the email address of the user who has forgotten their password.
    /// </summary>
    [EmailAddress]
    [Required]
    public required string Email { get; init; }
}
