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
        // Remove this normalisation when we have a normalised column
        // Cannot specify a culture as this cannot be translated to SQL.
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning disable CA1304 // Specify CultureInfo
#pragma warning disable MA0011 // IFormatProvider is missing
#pragma warning disable CA1311 // Specify a culture or use an invariant version
        var normalised = email.Trim().ToUpperInvariant();
        return usersSet.FirstOrDefaultAsync(
            x => x.WorkEmail.ToUpper() == normalised,
            cancellationToken
        );
#pragma warning restore CA1311 // Specify a culture or use an invariant version
#pragma warning restore MA0011 // IFormatProvider is missing
#pragma warning restore CA1304 // Specify CultureInfo
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
    }
}
