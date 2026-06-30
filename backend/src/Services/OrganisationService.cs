using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services.Interfaces;
using UKPS.Api.Services.Results;

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

    public async Task<DeactivateOrganisationUserResult> DeactivateUser(
        int organisationId,
        int userId
    )
    {
        bool organisationExists = await dbContext.Organisations.AnyAsync(o =>
            o.Id == organisationId
        );

        if (!organisationExists)
        {
            return DeactivateOrganisationUserResult.OrganisationNotFound();
        }

        UserOrgMembership? membership = await dbContext
            .UserOrgMemberships.Include(m => m.User)
            .SingleOrDefaultAsync(m => m.OrganisationId == organisationId && m.UserId == userId);

        if (membership is null)
        {
            return DeactivateOrganisationUserResult.UserNotFound();
        }

        if (membership.Status == UserOrgStatus.Inactive)
        {
            return DeactivateOrganisationUserResult.AlreadyInactive();
        }

        membership.Status = UserOrgStatus.Inactive;

        await dbContext.SaveChangesAsync();

        return DeactivateOrganisationUserResult.Success(MapToUserListItemDto(membership));
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

    private static UserListItemDto MapToUserListItemDto(UserOrgMembership membership) =>
        new()
        {
            UserId = membership.User.Id,
            EmailAddress = membership.User.WorkEmail,
            Role = membership.UserRole,
            Status = membership.Status,
            LastActive = null,
        };
}
