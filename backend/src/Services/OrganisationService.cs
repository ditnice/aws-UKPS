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
        organisation.HeadOfficeAddress = organisationDetails.HeadOfficeAddress;
        organisation.HeadOfficeEmail = organisationDetails.HeadOfficeEmail;
        organisation.HeadOfficeTelephone = organisationDetails.HeadOfficeTelephone;

        await dbContext.SaveChangesAsync();

        return MapToDto(organisation);
    }

    public async Task<UserOrganisationMembershipDto?> UpdateUserOrganisationMembershipRole(
        int organisationId,
        int userId,
        UpdateUserOrganisationMembershipRoleCommandDto command,
        CancellationToken cancellationToken
    )
    {
        UserOrgMembership? userOrganisationMembership =
            await dbContext.UserOrgMemberships.FirstOrDefaultAsync(
                x => x.OrganisationId == organisationId && x.UserId == userId,
                cancellationToken
            );

        if (userOrganisationMembership is null)
        {
            return null;
        }

        if (userOrganisationMembership.UserRole == command.UserRole)
        {
            throw new BadRequestException("The user already has the specified organisation role.");
        }

        userOrganisationMembership.UserRole = command.UserRole;

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(userOrganisationMembership);
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

    private static UserOrganisationMembershipDto MapToDto(UserOrgMembership entity)
    {
        return new UserOrganisationMembershipDto
        {
            UserId = entity.UserId,
            OrganisationId = entity.OrganisationId,
            UserRole = entity.UserRole,
            Status = entity.Status,
            AllowedPharmaceuticalEntity = entity.AllowedPharmaceuticalEntity,
            CreatedAt = entity.CreatedAt,
        };
    }
}
