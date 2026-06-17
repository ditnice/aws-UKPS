using UKPS.Api.DTOs;

namespace UKPS.Api.Services;

public interface IOrganisationService
{
    Task<OrganisationDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
