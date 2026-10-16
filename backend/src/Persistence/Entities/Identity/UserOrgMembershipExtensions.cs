using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal static class UserOrgMembershipExtensions
{
    // Inactive only records a lack of recent activity, so an inactive champion is still contactable.
    private static readonly UserOrgMembershipStatus[] _eligibleChampionStatuses =
    [
        UserOrgMembershipStatus.Active,
        UserOrgMembershipStatus.Inactive,
    ];

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

    public static bool IsEligibleChampion(this UserOrgMembership membership) =>
        membership.UserRole == UserRole.Champion
        && _eligibleChampionStatuses.Contains(membership.Status);

    public static Task<bool> AnyOtherEligibleChampion(
        this DbSet<UserOrgMembership> dbSet,
        int organisationId,
        IReadOnlyCollection<int> excludedMembershipIds,
        CancellationToken cancellationToken
    ) =>
        dbSet.AnyAsync(
            x =>
                x.OrganisationId == organisationId
                && !excludedMembershipIds.Contains(x.Id)
                && x.UserRole == UserRole.Champion
                && _eligibleChampionStatuses.Contains(x.Status),
            cancellationToken
        );
}
