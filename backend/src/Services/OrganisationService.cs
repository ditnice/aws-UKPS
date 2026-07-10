using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Services;

// Type aliases for Result
using GetOrganisationResult = Common.Result<OrganisationDetailsDto, GetOrganisationByIdError>;
using UpdateOrganisationResult = Common.Result<
    OrganisationDetailsDto,
    UpdateOrganisationDetailsError
>;

internal sealed class OrganisationService : IOrganisationService
{
    public IOrganisationMembershipService Memberships { get; }

    private readonly AppDbContext _dbContext;
    private readonly IOrganisationAuthoriser _organisationAuthoriser;

    public OrganisationService(
        AppDbContext dbContext,
        IOrganisationMembershipService membershipService,
        IOrganisationAuthoriser organisationAuthoriser
    )
    {
        _dbContext = dbContext;
        Memberships = membershipService;
        _organisationAuthoriser = organisationAuthoriser;
    }

    public async Task<GetOrganisationResult> GetOrganisationById(int id)
    {
        if (!_organisationAuthoriser.CanPerformOperationOnOrganisation(Operation.Read, id))
        {
            return GetOrganisationResult.Err(new GetOrganisationByIdError.NotAllowed(id));
        }
        var organisation = await _dbContext
            .Organisations.AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == id);

        return organisation is null
            ? GetOrganisationResult.Err(new GetOrganisationByIdError.NotFound(id))
            : GetOrganisationResult.Ok(MapToDto(organisation));
    }

    public async Task<UpdateOrganisationResult> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails
    )
    {
        if (!_organisationAuthoriser.CanPerformOperationOnOrganisation(Operation.Update, id))
        {
            return UpdateOrganisationResult.Err(new UpdateOrganisationDetailsError.NotAllowed(id));
        }
        var organisation = await _dbContext.Organisations.SingleOrDefaultAsync(o => o.Id == id);

        if (organisation is null)
        {
            return UpdateOrganisationResult.Err(new UpdateOrganisationDetailsError.NotFound(id));
        }

        organisation.OrganisationName = organisationDetails.OrganisationName;
        organisation.HeadOfficeAddress = organisationDetails.HeadOfficeAddress;
        organisation.HeadOfficeEmail = organisationDetails.HeadOfficeEmail;
        organisation.HeadOfficeTelephone = organisationDetails.HeadOfficeTelephone;

        await _dbContext.SaveChangesAsync();

        return UpdateOrganisationResult.Ok(MapToDto(organisation));
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
