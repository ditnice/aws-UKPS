using UKPS.Api.DTOs;

namespace UKPS.Api.Services.Interfaces;

public interface IOrganisationService
{
    Task<OrganisationDetailsDto?> GetOrganisationById(int id);
}
