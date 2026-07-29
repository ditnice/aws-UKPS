using System.ComponentModel.DataAnnotations;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the information required to create a new user.
/// </summary>
public sealed record CreateUserRequestDto
{
    /// <summary>
    /// Gets the type of user to create.
    /// </summary>
    [Required]
    public required UserType UserType { get; init; }

    /// <summary>
    /// Gets the user's title (for example, Mr, Mrs, Ms, or Dr).
    /// </summary>
    [Required]
    public required string Title { get; init; }

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    [Required]
    public required string FirstName { get; init; }

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    [Required]
    public required string LastName { get; init; }

    /// <summary>
    /// Gets the user's job title.
    /// </summary>
    [Required]
    public required string JobTitle { get; init; }

    /// <summary>
    /// Gets the user's work telephone number.
    /// </summary>
    [Required]
    public required string WorkTelephone { get; init; }

    /// <summary>
    /// Gets the user's work email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string WorkEmail { get; init; }

    /// <summary>
    /// Gets the identifier of the organisation the user belongs to.
    /// </summary>
    [Required]
    public required int OrganisationId { get; init; }
}
