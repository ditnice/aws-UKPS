namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed record SeedingConfiguration
{
    public const string SectionName = "Seeding";

    public bool ReseedOnStartup { get; init; }
}
