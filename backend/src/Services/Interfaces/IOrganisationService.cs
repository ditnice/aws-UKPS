using UKPS.Api.DTOs;

namespace UKPS.Api.Services.Interfaces;

public interface IOrganisationService
{
    IOrganisationMembershipService Memberships { get; }
    Task<OrganisationDetailsDto?> GetOrganisationById(int id);
    Task<OrganisationDetailsDto?> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails
    );
}
