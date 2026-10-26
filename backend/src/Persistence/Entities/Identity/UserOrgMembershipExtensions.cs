using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence.Enums;

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
        return dbSet
            .Include(x => x.User)
            .Include(x => x.Organisation)
            .FirstOrDefaultAsync(
                x => x.OrganisationId == orgId && x.Id == membershipId,
                cancellationToken
            );
    }

    public static bool IsEnabledChampion(this UserOrgMembership membership) =>
        membership.UserRole == UserRole.Champion
        && UserOrgMembership.EnabledStatuses.Contains(membership.Status);

    public static IQueryable<UserOrgMembership> EnabledChampions(
        this IQueryable<UserOrgMembership> query,
        int organisationId
    ) =>
        query.Where(x =>
            x.OrganisationId == organisationId
            && x.UserRole == UserRole.Champion
            && UserOrgMembership.EnabledStatuses.Contains(x.Status)
        );

    public static Task<bool> AnyOtherEnabledChampion(
        this IQueryable<UserOrgMembership> query,
        int organisationId,
        IReadOnlyCollection<int> excludedMembershipIds,
        CancellationToken cancellationToken
    ) =>
        query
            .EnabledChampions(organisationId)
            .AnyAsync(x => !excludedMembershipIds.Contains(x.Id), cancellationToken);
}
