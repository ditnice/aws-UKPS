namespace UKPS.Api.Persistence;

internal static class WebApplicationMigrationExtensions
{
    public static async Task MigrateDatabase(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        DatabaseConfiguration? settings =
            app.Configuration.GetSection(DatabaseConfiguration.SectionName)
                .Get<DatabaseConfiguration>()
            ?? new DatabaseConfiguration();

        if (!settings.MigrateOnStartup)
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
