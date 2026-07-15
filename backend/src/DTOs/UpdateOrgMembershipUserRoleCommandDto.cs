using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

/// <summary>
/// Represents a Data Transfer Object (DTO) for updating the user role
/// in an organization membership.
/// </summary>
public record UpdateOrgMembershipUserRoleCommandDto
{
    /// <summary>
    /// Gets or initializes the user role to be updated in the organization membership.
    /// </summary>
    /// <value>
    /// The new <see cref="UserRole"/> to assign to the user.
    /// </value>
    public required UserRole UserRole { get; init; }
}
