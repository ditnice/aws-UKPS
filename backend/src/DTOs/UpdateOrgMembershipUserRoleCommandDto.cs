using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

public record UpdateOrgMembershipUserRoleCommandDto
{
    public required UserRole UserRole { get; init; }
}
