using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

/// <summary>
/// Represents the information required to create a new user.
/// </summary>
public sealed record CreateUserRequestDto
{
    /// <summary>
    /// Gets the user's username.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the type of user to create.
    /// </summary>
    public required UserType UserType { get; init; }

    /// <summary>
    /// Gets the user's title (for example, Mr, Mrs, Ms, or Dr).
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Gets the user's job title.
    /// </summary>
    public required string JobTitle { get; init; }

    /// <summary>
    /// Gets the user's work telephone number.
    /// </summary>
    public required string WorkTelephone { get; init; }

    /// <summary>
    /// Gets the user's work email address.
    /// </summary>
    public required string WorkEmail { get; init; }

    /// <summary>
    /// Gets the identifier of the organisation the user belongs to.
    /// </summary>
    public required int OrganisationId { get; init; }
}
