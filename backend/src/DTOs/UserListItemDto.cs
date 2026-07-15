using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

/// <summary>
/// Represents a limited summary of a user.
/// </summary>
public sealed record UserListItemDto
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Gets the email address of the user, if available.
    /// </summary>
    public string? EmailAddress { get; init; }

    /// <summary>
    /// Gets the role of the user within the system.
    /// </summary>
    public UserRole Role { get; init; }

    /// <summary>
    /// Gets the organizational status of the user.
    /// </summary>
    public UserOrgStatus Status { get; init; }

    /// <summary>
    /// Gets the date and time when the user was last active, if available.
    /// </summary>
    public DateTime? LastActive { get; init; }
}
