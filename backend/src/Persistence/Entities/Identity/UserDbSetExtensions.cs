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

    public static Task<User?> GetByPendingEmailOrDefault(
        this IQueryable<User> usersSet,
        string email,
        CancellationToken cancellationToken
    )
    {
        return usersSet.FirstOrDefaultAsync(
            x => x.NormalisedPendingWorkEmail == EmailDbFunctions.Normalise(email),
            cancellationToken
        );
    }
}
