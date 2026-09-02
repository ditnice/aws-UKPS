namespace UKPS.Api.Persistence.Data.Seeding;

internal static class WebApplicationSeedingExtensions
{
    public static async Task SeedData(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        SeedingOptions? settings = app
            .Configuration.GetSection(SeedingOptions.SectionName)
            .Get<SeedingOptions>();

        if (settings is null || !settings.ReseedOnStartup)
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        IHostApplicationLifetime lifetime =
            app.Services.GetRequiredService<IHostApplicationLifetime>();
        IDataSeeder? dataSeeder = scope.ServiceProvider.GetService<IDataSeeder>();
        if (dataSeeder is null)
            return;
        await dataSeeder.SeedData(settings, lifetime.ApplicationStopping);
    }
}
