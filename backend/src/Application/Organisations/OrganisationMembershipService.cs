using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.Organisations.Dtos;
using UKPS.Api.Application.Organisations.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using DeactivateUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Organisations.Dtos.OrganisationMembershipDto,
    UKPS.Api.Application.Organisations.Errors.OrganisationMembershipDeactivateUserError
>;
using UpdateUserRoleResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Organisations.Dtos.OrganisationMembershipDto,
    UKPS.Api.Application.Organisations.Errors.OrganisationMembershipUpdateUserRoleError
>;

namespace UKPS.Api.Application.Organisations;

internal sealed class OrganisationMembershipService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser
) : IOrganisationMembershipService
{
    public async Task<UpdateUserRoleResult> UpdateUserRole(
        int organisationId,
        int membershipId,
        UpdateOrgMembershipUserRoleCommandDto command,
        CancellationToken cancellationToken
    )
    {
        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Update,
                organisationId
            )
        )
        {
            var error = new OrganisationMembershipUpdateUserRoleError.NotAllowed(organisationId);
            return UpdateUserRoleResult.Err(error);
        }
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
        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Update,
                organisationId
            )
        )
        {
            var error = new OrganisationMembershipDeactivateUserError.NotAllowed(organisationId);
            return DeactivateUserResult.Err(error);
        }
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
