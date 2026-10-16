using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UKPS.ErdGenerator;

/// <summary>The validated contents of <c>erd-groups.yml</c>.</summary>
internal sealed partial class ErdGroupsConfig
{
    private const string ConfigHint = "documentation/database/erd-groups.yml";

    private static readonly string[] _allowedDirections = ["TB", "BT", "LR", "RL"];

    private ErdGroupsConfig(string direction, IReadOnlyList<ErdGroup> groups)
    {
        Direction = direction;
        Groups = groups;
    }

    public string Direction { get; }

    /// <summary>Groups in the order they are declared in the config file.</summary>
    public IReadOnlyList<ErdGroup> Groups { get; }

    public static ErdGroupsConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new ErdGenerationException($"ERD grouping config not found: {path}");
        }

        IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        ErdGroupsFile file;
        try
        {
            file =
                deserializer.Deserialize<ErdGroupsFile>(File.ReadAllText(path))
                ?? throw new ErdGenerationException($"ERD grouping config is empty: {path}");
        }
        catch (YamlException ex)
        {
            throw new ErdGenerationException(
                $"ERD grouping config could not be parsed ({path}): {ex.Message}",
                ex
            );
        }

        return Parse(file, path);
    }

    private static ErdGroupsConfig Parse(ErdGroupsFile file, string path)
    {
        List<string> errors = [];

        string direction = file.Direction ?? "TB";
        if (!_allowedDirections.Contains(direction, StringComparer.Ordinal))
        {
            errors.Add(
                $"direction must be one of {string.Join(", ", _allowedDirections)} (found '{direction}')."
            );
        }

        Dictionary<string, ErdGroupDefinition?> definitions =
            file.Groups ?? new Dictionary<string, ErdGroupDefinition?>(StringComparer.Ordinal);
        if (definitions.Count == 0)
        {
            errors.Add("No groups are defined under 'groups:'.");
        }

        List<ErdGroup> groups = [];
        Dictionary<string, string> tableOwners = new(StringComparer.Ordinal);

        foreach ((string key, ErdGroupDefinition? rawDefinition) in definitions)
        {
            ErdGroupDefinition definition = rawDefinition ?? new ErdGroupDefinition();
            definition.Tables ??= [];
            ValidateGroup(key, definition, errors);
            RegisterTables(key, definition, tableOwners, errors);

            groups.Add(
                new ErdGroup(
                    key,
                    definition.Title ?? key,
                    definition.Colour ?? "#ffffff",
                    (definition.Tables ?? []).Order(StringComparer.Ordinal).ToList()
                )
            );
        }

        if (errors.Count > 0)
        {
            throw new ErdGenerationException(
                $"ERD grouping config is invalid ({path}):{Environment.NewLine}"
                    + string.Join(Environment.NewLine, errors.Select(e => "  - " + e))
            );
        }

        return new ErdGroupsConfig(direction, groups);
    }

    private static void ValidateGroup(
        string key,
        ErdGroupDefinition definition,
        List<string> errors
    )
    {
        if (!GroupKeyPattern().IsMatch(key))
        {
            errors.Add(
                $"Group key '{key}' must be lower-case snake_case (letters, digits, underscores)."
            );
        }

        if (string.IsNullOrWhiteSpace(definition.Title))
        {
            errors.Add($"Group '{key}' has no title.");
        }

        if (definition.Colour is null || !ColourPattern().IsMatch(definition.Colour))
        {
            errors.Add($"Group '{key}' colour must be a hex colour like '#dbeafe'.");
        }

        if (definition.Tables is null || definition.Tables.Count == 0)
        {
            errors.Add($"Group '{key}' has no tables.");
        }
    }

    private static void RegisterTables(
        string key,
        ErdGroupDefinition definition,
        Dictionary<string, string> tableOwners,
        List<string> errors
    )
    {
        foreach (string table in definition.Tables ?? [])
        {
            if (tableOwners.TryGetValue(table, out string? existingOwner))
            {
                errors.Add(
                    $"Table '{table}' is assigned to more than one group ('{existingOwner}' and '{key}')."
                );
            }
            else
            {
                tableOwners[table] = key;
            }
        }
    }

    /// <summary>
    /// Checks that every table in the schema belongs to exactly one group and that every
    /// configured table exists in the schema.
    /// </summary>
    public void ValidateAgainst(SchemaModel schema)
    {
        HashSet<string> schemaTables = schema
            .Tables.Select(t => t.Name)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> groupedTables = Groups
            .SelectMany(g => g.Tables)
            .ToHashSet(StringComparer.Ordinal);

        List<string> unknown = groupedTables
            .Where(t => !schemaTables.Contains(t))
            .Order(StringComparer.Ordinal)
            .ToList();
        List<string> ungrouped = schemaTables
            .Where(t => !groupedTables.Contains(t))
            .Order(StringComparer.Ordinal)
            .ToList();

        if (unknown.Count == 0 && ungrouped.Count == 0)
        {
            return;
        }

        List<string> lines = [];
        if (ungrouped.Count > 0)
        {
            lines.Add("The following tables are not assigned to an ERD group:");
            lines.AddRange(ungrouped.Select(t => "  - " + t));
            lines.Add($"Add each table to a group in {ConfigHint} and re-run the generator.");
        }

        if (unknown.Count > 0)
        {
            if (lines.Count > 0)
            {
                lines.Add(string.Empty);
            }

            lines.Add("The following grouped tables do not exist in the EF Core model:");
            lines.AddRange(unknown.Select(t => "  - " + t));
            lines.Add($"Remove or rename them in {ConfigHint}.");
        }

        throw new ErdGenerationException(string.Join(Environment.NewLine, lines));
    }

    [GeneratedRegex("^[a-z][a-z0-9_]*$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex GroupKeyPattern();

    [GeneratedRegex("^#[0-9a-fA-F]{6}$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColourPattern();
}
