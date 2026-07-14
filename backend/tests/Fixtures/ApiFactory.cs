using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using UKPS.Api.Data.Seeding;

namespace UKPS.Api.Tests.Fixtures;

public sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
        builder.UseSetting(
            $"{SeedingConfiguration.SectionName}:{nameof(SeedingConfiguration.ReseedOnStartup)}",
            $"{false}"
        );
    }
}
