using UKPS.Api.DTOs;

namespace UKPS.Api.Services.Interfaces;

public interface IOrganisationMembershipService
{
    Task<OrganisationMembershipDto?> UpdateUserRole(
        int organisationId,
        int membershipId,
        UpdateOrgMembershipUserRoleCommandDto command,
        CancellationToken cancellationToken
    );
}
