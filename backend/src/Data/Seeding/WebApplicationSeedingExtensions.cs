using Microsoft.EntityFrameworkCore;

namespace UKPS.Api.Data.Seeding;

internal static class WebApplicationSeedingExtensions
{
    public static async Task SeedData(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        SeedingConfiguration? settings = app
            .Configuration.GetSection(SeedingConfiguration.SectionName)
            .Get<SeedingConfiguration>();

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
        await dataSeeder.SeedData(lifetime.ApplicationStopping);
    }
}
