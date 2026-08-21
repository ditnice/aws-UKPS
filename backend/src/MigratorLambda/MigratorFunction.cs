using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Seeding;

[assembly: LambdaSerializer(
    typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer)
)]

namespace MigratorLambda;

public sealed class MigratorFunction
{
    public static async Task FunctionHandler(string input, ILambdaContext context)
    {
        string secretArn =
            Environment.GetEnvironmentVariable("DB_SECRET_ARN")
            ?? throw new InvalidOperationException("DB_SECRET_ARN is not set.");

        context.Logger.LogInformation("Fetching DB credentials from Secrets Manager...");

        using AmazonSecretsManagerClient secretsClient = new();
        GetSecretValueResponse secretResponse = await secretsClient.GetSecretValueAsync(
            new GetSecretValueRequest { SecretId = secretArn }
        );

        DbSecret secret =
            JsonSerializer.Deserialize<DbSecret>(secretResponse.SecretString)
            ?? throw new InvalidOperationException("Failed to deserialise DB secret.");

        IConfiguration config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddInMemoryCollection(
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    [$"Database__{nameof(DatabaseConfiguration.Username)}"] = secret.Username,
                    [$"Database__{nameof(DatabaseConfiguration.Password)}"] = secret.Password,
                }
            )
            .Build();

        string DB_CONNECTION_STRING = DatabaseConnectionStringFactory.GetConnectionString(config);

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(DB_CONNECTION_STRING)
            .Options;

        await using AppDbContext dbContext = new(options);

        context.Logger.LogInformation("Starting migrations...");
        await new DatabaseMigrator(dbContext).MigrateAsync();
        context.Logger.LogInformation("Migrations completed successfully.");

        IConfigurationSection seedingConfigSection = config.GetSection("Seeding");
        SeedingConfiguration SeedingConfig =
            seedingConfigSection.Get<SeedingConfiguration>()
            ?? throw new InvalidOperationException("Seeding configuration is not set.");

        context.Logger.LogInformation("Starting data seeding...");
        await new DataSeederInMemory(new SeedDataWriter(dbContext)).SeedData(SeedingConfig);
        context.Logger.LogInformation("Data seeding completed successfully.");
    }

    private sealed class DbSecret
    {
        [JsonPropertyName("username")]
        public string Username { get; init; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; init; } = string.Empty;
    }
}
