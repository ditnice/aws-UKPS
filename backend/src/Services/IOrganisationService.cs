using UKPS.Api.DTOs;

namespace UKPS.Api.Services;

public interface IOrganisationService
{
    Task<OrganisationDetailsDto?> GetOrganisationById(int id);
}
