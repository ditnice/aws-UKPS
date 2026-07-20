using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

/// <summary>
/// Represents the details of a user.
/// </summary>
public sealed record UserDetailsDto
{
    /// <summary>
    /// Gets the user's username.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the user's type.
    /// </summary>
    public required UserType UserType { get; init; }

    /// <summary>
    /// Gets the user's title (for example, Mr, Mrs, Ms, or Dr), if available.
    /// </summary>
    public required string? Title { get; init; }

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Gets the user's job title, if available.
    /// </summary>
    public required string? JobTitle { get; init; }

    /// <summary>
    /// Gets the user's work telephone number, if available.
    /// </summary>
    public required string? WorkPhone { get; init; }

    /// <summary>
    /// Gets the user's work email address.
    /// </summary>
    public required string WorkEmail { get; init; }
}
