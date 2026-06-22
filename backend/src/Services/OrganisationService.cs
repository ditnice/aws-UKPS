using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Services;

internal sealed class OrganisationService(AppDbContext dbContext) : IOrganisationService
{
    public async Task<OrganisationDetailsDto?> GetOrganisationById(int id)
    {
        return await dbContext.Organisations
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new OrganisationDetailsDto
            {
                Id = o.Id,
                OrganisationName = o.OrganisationName,
                OrganisationType = o.OrganisationType,
                AllowedPharmaceuticalEntity = o.AllowedPharmaceuticalEntity,
                CountryOrRegion = o.CountryOrRegion,
                HeadOfficeAddress = o.HeadOfficeAddress,
                HeadOfficeEmail = o.HeadOfficeEmail,
                HeadOfficeTelephone = o.HeadOfficeTelephone,
                Status = o.Status,
                LastActive = o.LastActive,
                CreatedAt = o.CreatedAt,
            })
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
    }
}
