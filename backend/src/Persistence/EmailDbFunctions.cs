namespace UKPS.Api.Persistence;

/// <summary>
/// Database functions that can be used inside EF Core LINQ queries.
/// </summary>
/// <remarks>
/// <para>
/// The methods on this type are mapped to SQL functions in
/// <c>AppDbContext.OnModelCreating</c> via <c>HasDbFunction</c>. They exist only so that
/// EF Core can translate calls to them into SQL. They have no client-side implementation
/// and throw <see cref="NotSupportedException"/> if invoked outside a query.
/// </para>
/// </remarks>
public static class EmailDbFunctions
{
    /// <summary>
    /// Normalises an email address for case-insensitive comparison by calling the
    /// <c>ukps.normalise_email</c> database function.
    /// </summary>
    /// <param name="email">The email address to normalise.</param>
    /// <returns>The normalised email address, as computed by the database.</returns>
    /// <remarks>
    /// <para>
    /// This must produce the same result as the expression behind the generated
    /// <c>normalised_work_email</c> column. Comparing against that column, as in
    /// <c>x.NormalisedWorkEmail == EmailDbFunctions.Normalise(email)</c>, lets Postgres use
    /// the unique index <c>ix_app_user_normalised_work_email</c>.
    /// </para>
    /// <para>
    /// Only call this method inside an EF Core query. Calling it directly in application
    /// code always throws.
    /// </para>
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Always thrown when the method is called outside of an EF Core query, because
    /// the method is translated to SQL and has no client-side implementation.
    /// </exception>
    public static string Normalise(string email) =>
        throw new NotSupportedException("Translated to SQL by EF Core.");
}
