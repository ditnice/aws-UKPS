using Microsoft.EntityFrameworkCore;

namespace UKPS.Api.Persistence;

public interface IDatabaseMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken);
}

internal sealed class DatabaseMigrator(AppDbContext dbContext) : IDatabaseMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}

internal static class WebApplicationMigrationExtensions
{
    public static async Task MigrateDatabase(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        DatabaseConfiguration? settings = app
            .Configuration.GetSection(DatabaseConfiguration.SectionName)
            .Get<DatabaseConfiguration>();

        if (settings is null || !settings.MigrateOnStartup)
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        IHostApplicationLifetime lifetime =
            app.Services.GetRequiredService<IHostApplicationLifetime>();
        IDatabaseMigrator databaseMigrator =
            scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>();
        await databaseMigrator.MigrateAsync(lifetime.ApplicationStopping);
    }
}
