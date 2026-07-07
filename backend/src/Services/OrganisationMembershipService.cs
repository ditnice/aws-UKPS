using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;
using DeactivateUserResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationMembershipDto,
    UKPS.Api.Services.Errors.OrganisationMembershipDeactivateUserError
>;
using UpdateUserRoleResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationMembershipDto,
    UKPS.Api.Services.Errors.OrganisationMembershipUpdateUserRoleError
>;

namespace UKPS.Api.Services;

internal sealed class OrganisationMembershipService(AppDbContext dbContext)
    : IOrganisationMembershipService
{
    public async Task<UpdateUserRoleResult> UpdateUserRole(
        int organisationId,
        int membershipId,
        UpdateOrgMembershipUserRoleCommandDto command,
        CancellationToken cancellationToken
    )
    {
        var membership = await dbContext.UserOrgMemberships.FirstOrDefaultAsync(
            x => x.OrganisationId == organisationId && x.Id == membershipId,
            cancellationToken
        );
        if (membership is null)
        {
            var error = new OrganisationMembershipUpdateUserRoleError.NotFound(
                organisationId,
                membershipId
            );
            return UpdateUserRoleResult.Err(error);
        }
        membership.UserRole = command.UserRole;
        await dbContext.SaveChangesAsync(cancellationToken);
        return UpdateUserRoleResult.Ok(MapToDto(membership));
    }

    public async Task<DeactivateUserResult> DeactivateMembership(
        int organisationId,
        int membershipId,
        CancellationToken cancellationToken
    )
    {
        var membership = await dbContext.UserOrgMemberships.FirstOrDefaultAsync(
            x => x.OrganisationId == organisationId && x.Id == membershipId,
            cancellationToken
        );
        if (membership is null)
        {
            return DeactivateUserResult.Err(
                new OrganisationMembershipDeactivateUserError.NotFound()
            );
        }
        membership.Status = UserOrgStatus.Inactive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return DeactivateUserResult.Ok(MapToDto(membership));
    }

    private static OrganisationMembershipDto MapToDto(UserOrgMembership entity)
    {
        return new OrganisationMembershipDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            OrganisationId = entity.OrganisationId,
            UserRole = entity.UserRole,
            Status = entity.Status,
            AllowedPharmaceuticalEntity = entity.AllowedPharmaceuticalEntity,
            CreatedAt = entity.CreatedAt,
        };
    }
}
