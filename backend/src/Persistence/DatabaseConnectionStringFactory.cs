using Npgsql;

namespace UKPS.Api.Persistence;

internal static class DatabaseConnectionStringFactory
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string? configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            return configuredConnectionString;
        }

        DatabaseOptions settings =
            configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        EnsureConfigured(settings.Host, nameof(DatabaseOptions.Host));
        EnsureConfigured(settings.Name, nameof(DatabaseOptions.Name));
        EnsureConfigured(settings.Username, nameof(DatabaseOptions.Username));
        EnsureConfigured(settings.Password, nameof(DatabaseOptions.Password));

        return new NpgsqlConnectionStringBuilder
        {
            Host = settings.Host,
            Port = settings.Port ?? 5432,
            Database = settings.Name,
            Username = settings.Username,
            Password = settings.Password,
        }.ConnectionString;
    }

    private static void EnsureConfigured(string? value, string settingName)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Database connection string is not configured. Set ConnectionStrings:DefaultConnection or Database:{settingName}."
        );
    }
}
