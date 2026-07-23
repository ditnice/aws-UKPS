namespace UKPS.Api.Persistence;

internal sealed record DatabaseConfiguration
{
    public const string SectionName = "Database";
    public bool MigrateOnStartup { get; init; }
}
