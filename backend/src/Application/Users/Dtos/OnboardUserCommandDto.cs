using System.ComponentModel.DataAnnotations;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the data required to onboard a new user.
/// </summary>
public record OnboardUserCommandDto
{
    /// <summary>
    /// Gets the full name of the user to onboard.
    /// </summary>
    [Required]
    public required string FullName { get; init; }

    /// <summary>
    /// Gets the contact number of the user to onboard.
    /// </summary>
    [Required]
    [PhoneNumber]
    public required string ContactNumber { get; init; }

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
