using UKPS.Api.Common;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Errors;

namespace UKPS.Api.Services.Interfaces;

public interface IOrganisationService
{
    Task<Result<OrganisationDetailsDto, GetOrganisationByIdError>> GetOrganisationById(int id);
    Task<Result<OrganisationDetailsDto, UpdateOrganisationDetailsError>> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails
    );
}
