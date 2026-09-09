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
        return usersSet.FirstOrDefaultAsync(x => x.WorkEmail == email, cancellationToken);
    }
}
