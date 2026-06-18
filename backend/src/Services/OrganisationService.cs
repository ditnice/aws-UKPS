using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;

namespace UKPS.Api.Services;

internal sealed class OrganisationService(AppDbContext dbContext) : IOrganisationService
{
    public async Task<OrganisationDetailsDto?> GetOrganisationById(int id)
    {
        Organisation? organisation = await dbContext.Organisations
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == id)
            .ConfigureAwait(false);

        return (organisation is null) ? null
        : new OrganisationDetailsDto
        {
            Id = organisation.Id,
            OrganisationName = organisation.OrganisationName,
            OrganisationType = organisation.OrganisationType,
            AllowedPharmaceuticalEntity = organisation.AllowedPharmaceuticalEntity,
            CountryOrRegion = organisation.CountryOrRegion,
            HeadOfficeAddress = organisation.HeadOfficeAddress,
            HeadOfficeEmail = organisation.HeadOfficeEmail,
            HeadOfficeTelephone = organisation.HeadOfficeTelephone,
            Status = organisation.Status,
            LastActive = organisation.LastActive,
            CreatedAt = organisation.CreatedAt,
        };
    }
}
