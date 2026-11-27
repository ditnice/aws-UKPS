using Microsoft.EntityFrameworkCore;

namespace UKPS.Api.Persistence.Entities.Identity;

internal static class UserDbSetExtensions
{
    public static Task<User?> GetByEmailOrDefault(
        this IQueryable<User> usersSet,
        string email,
        CancellationToken cancellationToken
    )
    {
        return usersSet.FirstOrDefaultAsync(
            x => x.NormalisedWorkEmail == EmailDbFunctions.Normalise(email),
            cancellationToken
        );
    }

    public static async Task<User?> GetByPendingEmailOrDefault(
        this IQueryable<User> usersSet,
        string email,
        DateTime currentTime,
        TimeSpan timeout,
        CancellationToken cancellationToken,
        bool includeExpiredPendingEmail = false
    )
    {
        var user = await usersSet
            .Where(PendingEmailChange.HasActivePendingEmail(currentTime, timeout))
            .FirstOrDefaultAsync(
                x => x.NormalisedPendingWorkEmail == EmailDbFunctions.Normalise(email),
                cancellationToken
            );

        if (user is not null || !includeExpiredPendingEmail)
        {
            return user;
        }

        var expiredMatches = await usersSet
            .Where(x => x.NormalisedPendingWorkEmail == EmailDbFunctions.Normalise(email))
            .Take(2)
            .ToListAsync(cancellationToken);

        // If there is more than one we just return null. This means that in this unlikely edge case, users won't get an error telling them it's expired directly on login.
        return expiredMatches.Count == 1 ? expiredMatches[0] : null;
    }
}
