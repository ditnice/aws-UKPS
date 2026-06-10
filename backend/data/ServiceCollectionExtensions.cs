using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UKPS.Data;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers UkpsDbContext with:
    ///   - Npgsql provider
    ///   - snake_case naming convention to match PostgreSQL standards
    ///   - Connection pooling configured via the connection string
    ///     (use Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20 in the connection string)
    ///   - Explicit ukps schema on all tables (configured per entity)
    /// </summary>
    public static IServiceCollection AddUkpsData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Ukps")
            ?? throw new InvalidOperationException(
                "Connection string 'Ukps' not found. " +
                "Ensure it is configured in appsettings with connection pooling parameters: " +
                "Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20");

        services.AddDbContext<UkpsDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    // Descriptive migration assembly name for clarity in production
                    npgsql.MigrationsAssembly("UKPS.Data");

                    // Enable retry on transient failures (connection drops, pool exhaustion)
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                })
                // Maps PascalCase C# properties to snake_case PostgreSQL columns
                // e.g. RecordTitle -> record_title, CreatedAt -> created_at
                .UseSnakeCaseNamingConvention());

        return services;
    }
}
