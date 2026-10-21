using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence;
using UKPS.ErdGenerator;

const string SourceDescription =
    "EF Core model (AppDbContext) + documentation/database/erd-groups.yml";

GeneratorOptions? options = GeneratorOptions.Parse(args);
if (options is null)
{
    GeneratorOptions.PrintUsage();
    return 2;
}

try
{
    ErdGroupsConfig config = ErdGroupsConfig.Load(options.GroupsPath);

    using AppDbContext context = CreateDesignTimeContext();
    SchemaModel schema = SchemaModelReader.Read(context.Model);
    config.ValidateAgainst(schema);

    if (context.Database.HasPendingModelChanges())
    {
        Console.Error.WriteLine(
            "warning: the EF Core model has changes that are not yet captured in a migration. "
                + "The ERD reflects the model, not the latest migration."
        );
    }

    List<(string Path, string Content)> outputs =
    [
        (options.OutputPath, MermaidErdWriter.Write(schema, config, SourceDescription)),
    ];
    if (options.HtmlPath is not null)
    {
        outputs.Add((options.HtmlPath, ErdViewerWriter.Write(schema, config)));
    }

    if (options.Check)
    {
        return CheckUpToDate(outputs);
    }

    foreach ((string path, string content) in outputs)
    {
        WriteFile(path, content);
    }

    Console.WriteLine(
        $"Generated ERD with {schema.Tables.Count} tables, {schema.Relationships.Count} relationships "
            + $"and {config.Groups.Count} groups: {string.Join(", ", outputs.Select(o => o.Path))}"
    );
    return 0;
}
catch (ErdGenerationException ex)
{
    Console.Error.WriteLine("ERD generation failed.");
    Console.Error.WriteLine();
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static int CheckUpToDate(List<(string Path, string Content)> outputs)
{
    List<string> stale = [];
    foreach ((string path, string content) in outputs)
    {
        string existing = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        if (!string.Equals(existing, content, StringComparison.Ordinal))
        {
            stale.Add(path);
        }
    }

    if (stale.Count > 0)
    {
        Console.Error.WriteLine(
            $"ERD is out of date: {string.Join(", ", stale)}. "
                + "Run ./scripts/generate-erd.sh and commit the result."
        );
        return 1;
    }

    Console.WriteLine($"ERD is up to date: {string.Join(", ", outputs.Select(o => o.Path))}");
    return 0;
}

static void WriteFile(string path, string content)
{
    string directory =
        Path.GetDirectoryName(Path.GetFullPath(path)) ?? Directory.GetCurrentDirectory();
    Directory.CreateDirectory(directory);
    File.WriteAllText(path, content);
}

/// <summary>
/// Builds the context exactly as <c>Program.cs</c> does (Npgsql + snake_case naming) so the
/// model is identical to production, but never opens a connection: reading
/// <see cref="DbContext.Model"/> is purely in-memory.
/// </summary>
static AppDbContext CreateDesignTimeContext()
{
    DbContextOptions<AppDbContext> dbOptions = new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql("Host=localhost;Database=erd_generator;Username=erd;Password=erd")
        .UseSnakeCaseNamingConvention()
        .Options;

    return new AppDbContext(dbOptions);
}
