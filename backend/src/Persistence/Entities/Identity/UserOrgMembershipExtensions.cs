using Microsoft.EntityFrameworkCore;

namespace UKPS.Api.Persistence.Entities.Identity;

internal static class UserOrgMembershipExtensions
{
    public static Task<UserOrgMembership?> GetByOrgAndMembershipId(
        this DbSet<UserOrgMembership> dbSet,
        int orgId,
        int membershipId,
        CancellationToken cancellationToken
    )
    {
        return dbSet.FirstOrDefaultAsync(
            x => x.OrganisationId == orgId && x.Id == membershipId,
            cancellationToken
        );
    }
}
