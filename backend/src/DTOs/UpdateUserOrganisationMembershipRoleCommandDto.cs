using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

public record UpdateUserOrganisationMembershipRoleCommandDto
{
    public required UserRole UserRole { get; init; }
}
