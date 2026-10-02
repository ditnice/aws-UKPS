using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents a limited summary of a user.
/// </summary>
public sealed record UserListItemDto
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public required int? UserId { get; init; }

    /// <summary>
    /// The optional lasted active registration request associated with the user.
    /// </summary>
    public required int? RegistrationRequestId { get; init; }

    /// <summary>
    /// Gets the email address of the user, if available.
    /// </summary>
    public required string EmailAddress { get; init; }

    /// <summary>
    /// Gets the role of the user within the system.
    /// </summary>
    public required UserRole Role { get; init; }

    /// <summary>
    /// Gets the organisational status of the user.
    /// </summary>
    public required UserOrgStatus Status { get; init; }

    /// <summary>
    /// Gets the date and time when the user was last active, if available.
    /// </summary>
    public required DateTime? LastActive { get; init; }

    /// <summary>
    /// Gets the actions that can be performed by the current user.
    /// </summary>
    public required IReadOnlyCollection<UserMembershipAction> Actions { get; init; }
}
