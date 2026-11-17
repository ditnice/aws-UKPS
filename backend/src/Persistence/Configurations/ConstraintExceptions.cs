using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace UKPS.Api.Persistence.Configurations;

internal static class ConstraintExceptions
{
    public static bool IsUniqueEmailException(DbUpdateException updateException)
    {
        return UpdateExceptionMatchesConstraint(
            updateException,
            ConstraintNames.UserUniqueNormalisedEmail
        );
    }

    internal static bool IsUniquePendingEmailException(DbUpdateException updateException)
    {
        return UpdateExceptionMatchesConstraint(
            updateException,
            ConstraintNames.UserUniqueNormalisedPendingEmail
        );
    }

    private static bool UpdateExceptionMatchesConstraint(
        DbUpdateException updateException,
        string constraint
    )
    {
        return updateException.InnerException is PostgresException postgresException
            && string.Equals(
                postgresException.ConstraintName,
                constraint,
                StringComparison.Ordinal
            );
    }
}
