using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using UKPS.Api.Persistence;

namespace UKPS.Api.Tests.Persistence;

public sealed class WebApplicationMigrationExtensionsTests
{
    [Fact]
    public async Task MigrateDatabase_ShouldThrow_WhenApplicationIsNull()
    {
        WebApplication? app = null;

        Func<Task> act = () => app!.MigrateDatabase();

        await Should.ThrowAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task MigrateDatabase_ShouldNotMigrate_WhenConfigurationIsMissing()
    {
        IDatabaseMigrator migrator = Substitute.For<IDatabaseMigrator>();

        using WebApplication app = CreateApplication(
            migrator,
            new Dictionary<string, string?>(StringComparer.Ordinal)
        );

        await app.MigrateDatabase();

        await migrator.DidNotReceive().MigrateAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MigrateDatabase_ShouldNotMigrate_WhenMigrateOnStartupIsFalse()
    {
        IDatabaseMigrator migrator = Substitute.For<IDatabaseMigrator>();

        using WebApplication app = CreateApplication(
            migrator,
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [$"{DatabaseOptions.SectionName}:MigrateOnStartup"] = "false",
            }
        );

        await app.MigrateDatabase();

        await migrator.DidNotReceive().MigrateAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MigrateDatabase_ShouldMigrate_WhenMigrateOnStartupIsTrue()
    {
        IDatabaseMigrator migrator = Substitute.For<IDatabaseMigrator>();

        using WebApplication app = CreateApplication(
            migrator,
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [$"{DatabaseOptions.SectionName}:MigrateOnStartup"] = "true",
            }
        );

        await app.MigrateDatabase();

        await migrator.Received(1).MigrateAsync(Arg.Any<CancellationToken>());
    }

    private static WebApplication CreateApplication(
        IDatabaseMigrator migrator,
        Dictionary<string, string?> configuration
    )
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.Configuration.AddInMemoryCollection(configuration);

        builder.Services.AddSingleton(migrator);

        return builder.Build();
    }
}
