using System.ComponentModel.DataAnnotations;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the information required to register a new user.
/// </summary>
public sealed record RegisterUserCommandDto
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
    [PhoneNumber]
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// Gets the name of the organisation the user is requesting access to.
    /// </summary>
    [Required]
    public required int OrganisationId { get; init; }
}
