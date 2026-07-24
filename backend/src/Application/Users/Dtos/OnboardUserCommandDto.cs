using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the data required to onboard a new user.
/// </summary>
public record OnboardUserCommandDto
{
    /// <summary>
    /// Gets the email address of the user to onboard.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string NewUserEmail { get; init; }

    /// <summary>
    /// Specifies the organisation that the new user will be created for.
    /// </summary>
    [Required]
    public required int OrganisationId { get; init; }
}
