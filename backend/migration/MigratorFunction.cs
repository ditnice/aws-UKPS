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
        ArgumentNullException.ThrowIfNull(context);

        using var cancellationTokenSource = new CancellationTokenSource(context.RemainingTime);

        string secretArn =
            Environment.GetEnvironmentVariable("DB_SECRET_ARN")
            ?? throw new InvalidOperationException("DB_SECRET_ARN is not set.");

        context.Logger.LogInformation("Fetching DB credentials from Secrets Manager...");

        using AmazonSecretsManagerClient secretsClient = new();
        GetSecretValueResponse secretResponse = await secretsClient.GetSecretValueAsync(
            new GetSecretValueRequest { SecretId = secretArn },
            cancellationTokenSource.Token
        );

        DbSecret secret =
            JsonSerializer.Deserialize<DbSecret>(secretResponse.SecretString)
            ?? throw new InvalidOperationException("Failed to deserialise DB secret.");

        IConfiguration config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddInMemoryCollection(
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    [$"Database__{nameof(DatabaseOptions.Username)}"] = secret.Username,
                    [$"Database__{nameof(DatabaseOptions.Password)}"] = secret.Password,
                }
            )
            .Build();

        string dbConnectionString = DatabaseConnectionStringFactory.GetConnectionString(config);

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dbConnectionString)
            .Options;

        await using AppDbContext dbContext = new(options);

        SeedingOptions seedingOptions = config.GetSection("Seeding").Get<SeedingOptions>()
            ?? throw new InvalidOperationException("Seeding configuration is not set.");

        if (seedingOptions.ReseedOnStartup)
        {
            context.Logger.LogInformation("Seeding enabled - skipping migrations.");
            context.Logger.LogInformation("Starting data seeding...");
            await new DataSeederInMemory(new SeedDataWriter(dbContext)).SeedData(seedingOptions, cancellationTokenSource.Token);
            context.Logger.LogInformation("Data seeding completed successfully.");
        }
        else
        {
            context.Logger.LogInformation("Seeding skipped - starting migrations...");
            await new DatabaseMigrator(dbContext).MigrateAsync(cancellationTokenSource.Token);
            context.Logger.LogInformation("Migrations completed successfully.");
        }
    }

    private sealed class DbSecret
    {
        [JsonPropertyName("username")]
        public string Username { get; init; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; init; } = string.Empty;
    }
}
