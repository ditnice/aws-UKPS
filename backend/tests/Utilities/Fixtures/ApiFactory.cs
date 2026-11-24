using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Seeding;
using UKPS.Api.Tests.Utilities.MockInternalServices;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Tests.Utilities.Fixtures;

public sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    public TestAuthenticationOptions AuthOptions { get; } = new TestAuthenticationOptions();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureAppConfiguration(
            (context, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>(StringComparer.Ordinal)
                    {
                        [$"{EmailOptions.SectionName}:{nameof(EmailOptions.ListenerEnabled)}"] =
                            $"{false}",
                        [
                            $"{CognitoOptions.SectionName}:{nameof(CognitoOptions.ServiceUrlOverride)}"
                        ] = "https://validurl.com",
                        [$"{CognitoOptions.SectionName}:{nameof(CognitoOptions.Region)}"] =
                            "eu-west-2",
                    }
                );
            }
        );
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(AuthOptions);
            services.AddSingleton(_ => new MockEmailService().Mock);
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme,
                    _ => { }
                );
        });

        builder.ConfigureRequiredOptions();
        builder.UseSetting("AWS:LoadSecrets", $"{false}");
        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
        builder.UseSetting(
            $"{SeedingOptions.SectionName}:{nameof(SeedingOptions.ReseedOnStartup)}",
            $"{false}"
        );
        builder.UseSetting(
            $"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.MigrateOnStartup)}",
            $"{true}"
        );
        builder.UseSetting(
            $"{DevAuthenticationOptions.SectionName}:{nameof(DevAuthenticationOptions.IsEnabled)}",
            $"{true}"
        );
        builder.UseSetting(
            $"{EmailOptions.SectionName}:{nameof(EmailOptions.ListenerEnabled)}",
            $"{false}"
        );
        builder.ConfigureLogging(logging =>
            logging.AddFilter(
                "Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware",
                LogLevel.Error
            )
        );
    }
}
