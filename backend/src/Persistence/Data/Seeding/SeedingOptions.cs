namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed record SeedingOptions
{
    public const string SectionName = "Seeding";

    public bool ReseedOnStartup { get; init; }

    public string? SeedUsersJson { get; init; }
}
