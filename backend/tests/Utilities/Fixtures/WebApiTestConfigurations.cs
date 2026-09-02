using Microsoft.AspNetCore.Hosting;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Seeding;

namespace UKPS.Api.Tests.Utilities.Fixtures;

public static class WebApiTestConfigurations
{
    public static void ConfigureNoDatabase(this IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureDatabase(
            connectionString: null,
            reseedOnStartup: false,
            migrateOnStartup: false
        );
    }

    public static void ConfigureWithDatabase(this IWebHostBuilder builder, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureDatabase(connectionString, reseedOnStartup: false, migrateOnStartup: true);
    }

    private static void ConfigureDatabase(
        this IWebHostBuilder builder,
        string? connectionString,
        bool reseedOnStartup,
        bool migrateOnStartup
    )
    {
        if (connectionString is not null)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
        }
        builder.UseSetting(
            $"{SeedingOptions.SectionName}:{nameof(SeedingOptions.ReseedOnStartup)}",
            $"{reseedOnStartup}"
        );
        builder.UseSetting(
            $"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.MigrateOnStartup)}",
            $"{migrateOnStartup}"
        );
    }
}
