using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Services;

internal sealed class OrganisationMembershipService(AppDbContext dbContext)
    : IOrganisationMembershipService
{
    public async Task<OrganisationMembershipDto?> UpdateUserRole(
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
            return null;
        membership.UserRole = command.UserRole;
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(membership);
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
