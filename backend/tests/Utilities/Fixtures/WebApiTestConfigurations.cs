using Microsoft.AspNetCore.Hosting;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Seeding;
using UKPS.Api.WebApi.Validators;

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

    /// <summary>
    /// Replaces the <c>appsettings.json</c> placeholder values with test values, so that
    /// options marked with <see cref="RequiredConfigurationAttribute"/> pass startup validation.
    /// </summary>
    public static void ConfigureRequiredOptions(this IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        foreach ((string key, string value) in RequiredOptionValues)
        {
            builder.UseSetting(key, value);
        }
    }

    private static readonly (string Key, string Value)[] RequiredOptionValues =
    [
        ($"{CognitoOptions.SectionName}:{nameof(CognitoOptions.Region)}", "eu-west-2"),
        ($"{CognitoOptions.SectionName}:{nameof(CognitoOptions.UserPoolId)}", "eu-west-2_TestPool"),
        ($"{CognitoOptions.SectionName}:{nameof(CognitoOptions.ClientId)}", "test-client-id"),
        (
            $"{CognitoOptions.SectionName}:{nameof(CognitoOptions.ClientSecret)}",
            "test-client-secret"
        ),
        ($"{EmailOptions.SectionName}:{nameof(EmailOptions.FromAddress)}", "no-reply@example.com"),
        (
            $"{EmailOptions.SectionName}:{nameof(EmailOptions.QueueUrl)}",
            "https://sqs.eu-west-2.amazonaws.com/000000000000/test-queue"
        ),
    ];

    private static void ConfigureDatabase(
        this IWebHostBuilder builder,
        string? connectionString,
        bool reseedOnStartup,
        bool migrateOnStartup
    )
    {
        builder.ConfigureRequiredOptions();

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
