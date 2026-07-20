using Microsoft.EntityFrameworkCore;

namespace UKPS.Api.Persistence.Data;

internal static class WebApplicationMigrationExtensions
{
    public static async Task MigrateDatabase(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        IHostApplicationLifetime lifetime =
            app.Services.GetRequiredService<IHostApplicationLifetime>();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(lifetime.ApplicationStopping);
    }
}
