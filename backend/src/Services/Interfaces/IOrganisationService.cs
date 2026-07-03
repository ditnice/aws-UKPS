using UKPS.Api.DTOs;
using UKPS.Api.Services.Results;

namespace UKPS.Api.Services.Interfaces;

public interface IOrganisationService
{
    Task<OrganisationDetailsDto?> GetOrganisationById(int id);
    Task<OrganisationDetailsDto?> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails
    );
    Task<Result<UserListItemDto>> DeactivateUser(int organisationId, int userId);
}
