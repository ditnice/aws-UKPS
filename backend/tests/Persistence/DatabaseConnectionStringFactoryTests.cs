using Microsoft.Extensions.Configuration;
using Npgsql;
using Shouldly;
using UKPS.Api.Persistence;

namespace UKPS.Api.Tests.Persistence;

public sealed class DatabaseConnectionStringFactoryTests
{
    [Fact]
    public void GetConnectionString_ShouldReturnConfiguredConnectionString_WhenDefaultConnectionExists()
    {
        const string connectionString =
            "Host=localhost;Port=5432;Database=ukps_backend;Username=postgres;Password=postgres";
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                [$"{DatabaseOptions.SectionName}:Host"] = "aurora.example.com",
            }
        );

        string result = DatabaseConnectionStringFactory.GetConnectionString(configuration);

        result.ShouldBe(connectionString);
    }

    [Fact]
    public void GetConnectionString_ShouldBuildConnectionString_WhenDatabaseConfigurationExists()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [$"{DatabaseOptions.SectionName}:Host"] = "aurora.example.com",
                [$"{DatabaseOptions.SectionName}:Name"] = "ukpsdev_backend",
                [$"{DatabaseOptions.SectionName}:Password"] = "secret-password",
                [$"{DatabaseOptions.SectionName}:Port"] = "5432",
                [$"{DatabaseOptions.SectionName}:Username"] = "ukpsadmin",
            }
        );

        string result = DatabaseConnectionStringFactory.GetConnectionString(configuration);
        var builder = new NpgsqlConnectionStringBuilder(result);

        builder.Host.ShouldBe("aurora.example.com");
        builder.Port.ShouldBe(5432);
        builder.Database.ShouldBe("ukpsdev_backend");
        builder.Username.ShouldBe("ukpsadmin");
        builder.Password.ShouldBe("secret-password");
    }

    [Fact]
    public void GetConnectionString_ShouldThrow_WhenConnectionConfigurationIsMissing()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>(StringComparer.Ordinal)
        );

        Should.Throw<InvalidOperationException>(() =>
            DatabaseConnectionStringFactory.GetConnectionString(configuration)
        );
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
