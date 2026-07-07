using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Services;

internal sealed class OrganisationService : IOrganisationService
{
    public IOrganisationMembershipService Memberships { get; }

    private readonly AppDbContext _dbContext;

    public OrganisationService(
        AppDbContext dbContext,
        IOrganisationMembershipService membershipService
    )
    {
        _dbContext = dbContext;
        Memberships = membershipService;
    }

    public async Task<OrganisationDetailsDto?> GetOrganisationById(int id)
    {
        var organisation = await _dbContext
            .Organisations.AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == id);

        return organisation is null ? null : MapToDto(organisation);
    }

    public async Task<OrganisationDetailsDto?> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails
    )
    {
        var organisation = await _dbContext.Organisations.SingleOrDefaultAsync(o => o.Id == id);

        if (organisation is null)
        {
            return null;
        }

        organisation.OrganisationName = organisationDetails.OrganisationName;
        organisation.HeadOfficeAddress = organisationDetails.HeadOfficeAddress;
        organisation.HeadOfficeEmail = organisationDetails.HeadOfficeEmail;
        organisation.HeadOfficeTelephone = organisationDetails.HeadOfficeTelephone;

        await _dbContext.SaveChangesAsync();

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
            HeadOfficeAddress = organisation.HeadOfficeAddress,
            HeadOfficeEmail = organisation.HeadOfficeEmail,
            HeadOfficeTelephone = organisation.HeadOfficeTelephone,
            Status = organisation.Status,
            LastActive = organisation.LastActive,
            CreatedAt = organisation.CreatedAt,
        };
}
