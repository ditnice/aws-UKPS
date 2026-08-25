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
                [$"{DatabaseConfiguration.SectionName}:Host"] = "aurora.example.com",
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
                [$"{DatabaseConfiguration.SectionName}:Host"] = "aurora.example.com",
                [$"{DatabaseConfiguration.SectionName}:Name"] = "ukpsdev_backend",
                [$"{DatabaseConfiguration.SectionName}:Password"] = "secret-password",
                [$"{DatabaseConfiguration.SectionName}:Port"] = "5432",
                [$"{DatabaseConfiguration.SectionName}:RootCertificate"] =
                    "/app/certs/eu-west-2-bundle.pem",
                [$"{DatabaseConfiguration.SectionName}:Username"] = "ukpsadmin",
            }
        );

        string result = DatabaseConnectionStringFactory.GetConnectionString(configuration);
        var builder = new NpgsqlConnectionStringBuilder(result);

        builder.Host.ShouldBe("aurora.example.com");
        builder.Port.ShouldBe(5432);
        builder.Database.ShouldBe("ukpsdev_backend");
        builder.Username.ShouldBe("ukpsadmin");
        builder.Password.ShouldBe("secret-password");
        builder.SslMode.ShouldBe(SslMode.VerifyFull);
        builder.RootCertificate.ShouldBe("/app/certs/eu-west-2-bundle.pem");
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
