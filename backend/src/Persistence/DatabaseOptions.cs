using UKPS.Api.WebApi.Configuration;

namespace UKPS.Api.Persistence;

internal sealed record DatabaseOptions : IAppOptions
{
    public static string SectionName => "Database";
    public bool MigrateOnStartup { get; init; }
    public string? Host { get; init; }
    public string? Name { get; init; }
    public string? Password { get; init; }
    public int? Port { get; init; }
    public string? RootCertificate { get; init; }
    public string? Username { get; init; }
}
