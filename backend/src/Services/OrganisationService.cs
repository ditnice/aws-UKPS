using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Services;

internal sealed class OrganisationService(AppDbContext dbContext) : IOrganisationService
{
    public async Task<OrganisationDetailsDto?> GetOrganisationById(int id)
    {
        var organisation = await dbContext
            .Organisations.AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == id);

        return organisation is null ? null : MapToDto(organisation);
    }

    public async Task<OrganisationDetailsDto?> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails
    )
    {
        var organisation = await dbContext.Organisations.SingleOrDefaultAsync(o => o.Id == id);

        if (organisation is null)
        {
            return null;
        }

        organisation.OrganisationName = organisationDetails.OrganisationName;
        organisation.HeadOfficeAddressLine1 = organisationDetails.HeadOfficeAddressLine1;
        organisation.HeadOfficeAddressLine2 = organisationDetails.HeadOfficeAddressLine2;
        organisation.HeadOfficeTown = organisationDetails.HeadOfficeTown;
        organisation.HeadOfficeCounty = organisationDetails.HeadOfficeCounty;
        organisation.HeadOfficePostcode = organisationDetails.HeadOfficePostcode;
        organisation.HeadOfficeEmail = organisationDetails.HeadOfficeEmail;
        organisation.HeadOfficeTelephone = organisationDetails.HeadOfficeTelephone;

        await dbContext.SaveChangesAsync();

        return MapToDto(organisation);
    }

    private static OrganisationDetailsDto MapToDto(Organisation organisation) =>
        new()
        {
            Id = organisation.Id,
            OrganisationName = organisation.OrganisationName,
            OrganisationType = organisation.OrganisationType,
            AllowedPharmaceuticalEntity = organisation.AllowedPharmaceuticalEntity,
            CountryOrRegion = organisation.CountryOrRegion,
            HeadOfficeAddressLine1 = organisation.HeadOfficeAddressLine1,
            HeadOfficeAddressLine2 = organisation.HeadOfficeAddressLine2,
            HeadOfficeTown = organisation.HeadOfficeTown,
            HeadOfficeCounty = organisation.HeadOfficeCounty,
            HeadOfficePostcode = organisation.HeadOfficePostcode,
            HeadOfficeEmail = organisation.HeadOfficeEmail,
            HeadOfficeTelephone = organisation.HeadOfficeTelephone,
            Status = organisation.Status,
            LastActive = organisation.LastActive,
            CreatedAt = organisation.CreatedAt,
        };
}
