namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed record SeedingOptions
{
    public static string SectionName => "Seeding";

    public bool ReseedOnStartup { get; init; }

    public string? SeedUsersJson { get; init; }
}
