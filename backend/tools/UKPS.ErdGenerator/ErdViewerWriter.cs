using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UKPS.ErdGenerator;

/// <summary>
/// Produces a self-contained HTML viewer: the embedded template plus the schema and groups
/// as JSON. All filtering and Mermaid generation happens client-side.
/// </summary>
internal static class ErdViewerWriter
{
    private const string Placeholder = "/*__ERD_DATA__*/";
    private const string TemplateResource = "UKPS.ErdGenerator.ErdViewerTemplate.html";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = false,
    };

    public static string Write(SchemaModel schema, ErdGroupsConfig config)
    {
        var payload = new
        {
            config.Direction,
            Groups = config.Groups.Select(g => new
            {
                g.Key,
                g.Title,
                g.Colour,
                g.Tables,
            }),
            Tables = schema.Tables.Select(t => new
            {
                t.Name,
                Columns = t.Columns.Select(c => new
                {
                    c.Name,
                    Type = MermaidErdWriter.ToMermaidType(c.StoreType),
                    Nullable = c.IsNullable,
                    Pk = c.IsPrimaryKey,
                    Fk = c.IsForeignKey,
                    Uk = c.IsUnique,
                }),
            }),
            Relationships = schema.Relationships.Select(r => new
            {
                r.PrincipalTable,
                r.DependentTable,
                r.DependentColumns,
                r.IsRequired,
                r.IsOneToOne,
                r.IsIdentifying,
            }),
        };

        // The default encoder escapes '<' and '&', so the JSON is safe inside a <script> block.
        string json = JsonSerializer.Serialize(payload, _jsonOptions);
        string template = ReadTemplate();

        if (!template.Contains(Placeholder, StringComparison.Ordinal))
        {
            throw new ErdGenerationException("Viewer template is missing the data placeholder.");
        }

        return template.Replace(Placeholder, json, StringComparison.Ordinal);
    }

    private static string ReadTemplate()
    {
        using Stream stream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream(TemplateResource)
            ?? throw new ErdGenerationException(
                $"Embedded viewer template '{TemplateResource}' not found."
            );
        using StreamReader reader = new(stream);
        return reader.ReadToEnd().Replace("\r\n", "\n", StringComparison.Ordinal);
    }
}
