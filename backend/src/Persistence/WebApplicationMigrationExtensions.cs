namespace UKPS.Api.Persistence;

internal static class WebApplicationMigrationExtensions
{
    public static async Task MigrateDatabase(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        DatabaseOptions? settings =
            app.Configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

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
