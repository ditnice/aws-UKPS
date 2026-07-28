namespace UKPS.Api.Persistence;

/// <summary>
/// Defines a service responsible for applying database migrations.
/// </summary>
public interface IDatabaseMigrator
{
    /// <summary>
    /// Applies any pending database migrations asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the migration operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous migration operation.
    /// </returns>
    Task MigrateAsync(CancellationToken cancellationToken);
}
