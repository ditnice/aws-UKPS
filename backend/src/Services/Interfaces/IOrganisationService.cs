using UKPS.Api.DTOs;

namespace UKPS.Api.Services.Interfaces;

public interface IOrganisationService
{
    Task<OrganisationDetailsDto?> GetOrganisationById(int id);
    Task<OrganisationDetailsDto?> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails
    );

    Task<UserOrganisationMembershipDto?> UpdateUserOrganisationMembershipRole(
        int organisationId,
        int userId,
        UpdateUserOrganisationMembershipRoleCommandDto command,
        CancellationToken cancellationToken
    );
}
