using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

/// <summary>
/// Represents a Data Transfer Object (DTO) for updating the user role
/// in an organisation membership.
/// </summary>
public record UpdateOrgMembershipUserRoleCommandDto
{
    /// <summary>
    /// Gets or initialises the user role to be updated in the organisation membership.
    /// </summary>
    /// <value>
    /// The new <see cref="UserRole"/> to assign to the user.
    /// </value>
    public required UserRole UserRole { get; init; }
}
