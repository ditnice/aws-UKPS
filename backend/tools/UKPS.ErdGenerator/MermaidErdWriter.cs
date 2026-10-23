using System.Globalization;
using System.Text;

namespace UKPS.ErdGenerator;

/// <summary>
/// Renders a <see cref="SchemaModel"/> as a Mermaid <c>erDiagram</c>, with logical groups
/// expressed as subgraphs (Mermaid 11.17+) and coloured via <c>classDef</c>.
/// </summary>
internal static class MermaidErdWriter
{
    private const string Indent = "    ";

    /// <summary>Concise aliases for PostgreSQL store types containing spaces.</summary>
    private static readonly Dictionary<string, string> _typeAliases = new(StringComparer.Ordinal)
    {
        ["timestamp with time zone"] = "timestamptz",
        ["timestamp without time zone"] = "timestamp",
        ["time with time zone"] = "timetz",
        ["time without time zone"] = "time",
        ["double precision"] = "float8",
        ["character varying"] = "varchar",
        ["bit varying"] = "varbit",
    };

    public static string Write(SchemaModel schema, ErdGroupsConfig config, string sourceDescription)
    {
        Dictionary<string, SchemaTable> tablesByName = schema.Tables.ToDictionary(
            t => t.Name,
            StringComparer.Ordinal
        );

        StringBuilder sb = new();
        sb.Append("%% AUTO-GENERATED FILE. Do not edit directly.\n");
        sb.Append("%% Source: ").Append(sourceDescription).Append('\n');
        sb.Append("%% Regenerate with: ./scripts/generate-erd.sh\n");
        sb.Append("erDiagram\n");
        sb.Append(Indent).Append("direction ").Append(config.Direction).Append('\n');

        sb.Append('\n');
        foreach (ErdGroup group in config.Groups)
        {
            sb.Append(Indent)
                .Append("classDef ")
                .Append(group.Key)
                .Append(" fill:")
                .Append(group.Colour)
                .Append(",stroke:#374151,stroke-width:1px\n");
        }

        foreach (ErdGroup group in config.Groups)
        {
            sb.Append('\n');
            sb.Append(Indent)
                .Append("subgraph ")
                .Append(group.Key)
                .Append("[\"")
                .Append(group.Title)
                .Append("\"]\n");

            foreach (string tableName in group.Tables)
            {
                WriteEntity(sb, tablesByName[tableName], group.Key);
            }

            sb.Append(Indent).Append("end\n");
        }

        sb.Append('\n');
        sb.Append(Indent)
            .Append("%% Relationships: principal ||--o{ dependent : \"fk column(s)\"\n");
        foreach (SchemaRelationship relationship in schema.Relationships)
        {
            WriteRelationship(sb, relationship);
        }

        return sb.ToString();
    }

    private static void WriteEntity(StringBuilder sb, SchemaTable table, string groupKey)
    {
        sb.Append(Indent)
            .Append(Indent)
            .Append(table.Name)
            .Append(":::")
            .Append(groupKey)
            .Append(" {\n");

        foreach (SchemaColumn column in table.Columns)
        {
            sb.Append(Indent)
                .Append(Indent)
                .Append(Indent)
                .Append(ToMermaidType(column.StoreType))
                .Append(' ')
                .Append(column.Name);

            List<string> keys = [];
            if (column.IsPrimaryKey)
            {
                keys.Add("PK");
            }
            if (column.IsForeignKey)
            {
                keys.Add("FK");
            }
            if (column.IsUnique)
            {
                keys.Add("UK");
            }
            if (keys.Count > 0)
            {
                sb.Append(' ').Append(string.Join(", ", keys));
            }

            if (column.IsNullable)
            {
                sb.Append(" \"nullable\"");
            }

            sb.Append('\n');
        }

        sb.Append(Indent).Append(Indent).Append("}\n");
    }

    private static void WriteRelationship(StringBuilder sb, SchemaRelationship relationship)
    {
        // Principal side: exactly one when the FK is required, zero-or-one when optional.
        string principalEnd = relationship.IsRequired ? "||" : "|o";
        // Dependent side: zero-or-one for 1:1, zero-or-many otherwise.
        string dependentEnd = relationship.IsOneToOne ? "o|" : "o{";
        // Identifying relationships (FK is part of the dependent PK) use a solid line.
        string line = relationship.IsIdentifying ? "--" : "..";

        sb.Append(Indent)
            .Append(relationship.PrincipalTable)
            .Append(' ')
            .Append(principalEnd)
            .Append(line)
            .Append(dependentEnd)
            .Append(' ')
            .Append(relationship.DependentTable)
            .Append(" : \"")
            .Append(string.Join(", ", relationship.DependentColumns))
            .Append("\"\n");
    }

    /// <summary>
    /// Mermaid attribute types must be a single token, so multi-word PostgreSQL types are
    /// aliased (e.g. <c>timestamp with time zone</c> becomes <c>timestamptz</c>).
    /// </summary>
    internal static string ToMermaidType(string storeType)
    {
        string baseType = storeType;
        string suffix = string.Empty;

        int parenthesis = storeType.IndexOf('(', StringComparison.Ordinal);
        if (parenthesis >= 0)
        {
            baseType = storeType[..parenthesis];
            suffix = storeType[parenthesis..];
        }

        int bracket = baseType.IndexOf('[', StringComparison.Ordinal);
        string arraySuffix = string.Empty;
        if (bracket >= 0)
        {
            arraySuffix = baseType[bracket..];
            baseType = baseType[..bracket];
        }

        baseType = baseType.Trim();
        if (_typeAliases.TryGetValue(baseType, out string? alias))
        {
            baseType = alias;
        }

        string result = string.Create(
            CultureInfo.InvariantCulture,
            $"{baseType}{suffix}{arraySuffix}"
        );
        return result.Replace(' ', '_').Replace(',', '_');
    }
}
