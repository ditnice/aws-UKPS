using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.Organisations.Dtos;
using UKPS.Api.Application.Organisations.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using DeactivateUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Organisations.Dtos.OrganisationMembershipDto,
    UKPS.Api.Application.Organisations.Errors.OrganisationMembershipDeactivateUserError
>;
using ReactivateUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Organisations.Dtos.OrganisationMembershipDto,
    UKPS.Api.Application.Organisations.Errors.OrganisationMembershipReactivateUserError
>;
using UpdateUserRoleResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Organisations.Dtos.OrganisationMembershipDto,
    UKPS.Api.Application.Organisations.Errors.OrganisationMembershipUpdateUserRoleError
>;

namespace UKPS.Api.Application.Organisations;

internal sealed class OrganisationMembershipService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser,
    ICurrentUserInfoService currentUserInfoService
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
        var membership = await dbContext
            .UserOrgMemberships.Include(x => x.User)
            .FirstOrDefaultAsync(
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
        if (IsCurrentUsersOwnMembership(membership))
        {
            return UpdateUserRoleResult.Err(
                new OrganisationMembershipUpdateUserRoleError.CannotChangeOwnRole(membershipId)
            );
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
        var membership = await dbContext.UserOrgMemberships.GetByOrgAndMembershipId(
            organisationId,
            membershipId,
            cancellationToken
        );
        if (membership is null)
        {
            return DeactivateUserResult.Err(
                new OrganisationMembershipDeactivateUserError.NotFound()
            );
        }
        if (IsCurrentUsersOwnMembership(membership))
        {
            return DeactivateUserResult.Err(
                new OrganisationMembershipDeactivateUserError.CannotDeactivateSelf(membershipId)
            );
        }
        var result = membership.TryDeactivate();
        if (!result.Success)
        {
            return DeactivateUserResult.Err(
                new OrganisationMembershipDeactivateUserError.NotAllowedInCurrentState(result)
            );
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return DeactivateUserResult.Ok(MapToDto(membership));
    }

    public async Task<ReactivateUserResult> ReactivateMembership(
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
            var error = new OrganisationMembershipReactivateUserError.NotAllowed();
            return ReactivateUserResult.Err(error);
        }
        var membership = await dbContext.UserOrgMemberships.GetByOrgAndMembershipId(
            organisationId,
            membershipId,
            cancellationToken
        );
        if (membership is null)
        {
            return ReactivateUserResult.Err(
                new OrganisationMembershipReactivateUserError.NotFound()
            );
        }
        var result = membership.TryReactivate();
        if (!result.Success)
        {
            return ReactivateUserResult.Err(
                new OrganisationMembershipReactivateUserError.NotAllowedInCurrentState(result)
            );
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return ReactivateUserResult.Ok(MapToDto(membership));
    }

    // Users must not be able to change their own role or deactivate themselves, regardless of
    // the permissions their role would otherwise grant them over the organisation.
    private bool IsCurrentUsersOwnMembership(UserOrgMembership membership) =>
        currentUserInfoService.IsCurrentUser(membership.User!.WorkEmail);

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
