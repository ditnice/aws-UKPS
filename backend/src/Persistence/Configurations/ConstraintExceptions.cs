using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace UKPS.Api.Persistence.Configurations;

internal static class ConstraintExceptions
{
    public static bool IsUniqueEmailException(DbUpdateException updateException)
    {
        string[] uniqueEmailConstraintNames =
        [
            ConstraintNames.UserUniqueEmail,
            ConstraintNames.UserUniqueNormalisedEmail,
        ];
        return updateException.InnerException is PostgresException postgresException
            && uniqueEmailConstraintNames.Any(c =>
                string.Equals(postgresException.ConstraintName, c, StringComparison.Ordinal)
            );
    }
}
