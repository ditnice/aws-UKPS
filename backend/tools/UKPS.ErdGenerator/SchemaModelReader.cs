using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UKPS.ErdGenerator;

/// <summary>
/// Reads the effective relational schema from an EF Core <see cref="IModel"/> using the
/// relational model (tables, columns, constraints) rather than CLR entity shapes, so that
/// Fluent API configuration, naming conventions and JSON-mapped complex types are respected.
/// </summary>
internal static class SchemaModelReader
{
    public static SchemaModel Read(IModel model)
    {
        IRelationalModel relationalModel = model.GetRelationalModel();

        List<SchemaTable> tables = [];
        List<SchemaRelationship> relationships = [];

        foreach (
            ITable table in relationalModel.Tables.OrderBy(t => t.Name, StringComparer.Ordinal)
        )
        {
            tables.Add(ReadTable(table));
            relationships.AddRange(ReadRelationships(table));
        }

        relationships.Sort(CompareRelationships);

        return new SchemaModel(tables, relationships);
    }

    private static SchemaTable ReadTable(ITable table)
    {
        HashSet<string> primaryKeyColumns = PrimaryKeyColumns(table);

        HashSet<string> foreignKeyColumns = table
            .ForeignKeyConstraints.SelectMany(fk => fk.Columns)
            .Select(c => c.Name)
            .ToHashSet(StringComparer.Ordinal);

        HashSet<string> uniqueColumns = table
            .UniqueConstraints.Where(uc => !uc.GetIsPrimaryKey() && uc.Columns.Count == 1)
            .Select(uc => uc.Columns[0].Name)
            .Concat(
                table
                    .Indexes.Where(ix => ix.IsUnique && ix.Columns.Count == 1)
                    .Select(ix => ix.Columns[0].Name)
            )
            .ToHashSet(StringComparer.Ordinal);

        List<SchemaColumn> columns = table
            .Columns.Select(column => new SchemaColumn(
                column.Name,
                column.StoreType,
                column.IsNullable,
                IsPrimaryKey: primaryKeyColumns.Contains(column.Name),
                IsForeignKey: foreignKeyColumns.Contains(column.Name),
                IsUnique: uniqueColumns.Contains(column.Name)
            ))
            .ToList();

        return new SchemaTable(table.Name, columns);
    }

    private static IEnumerable<SchemaRelationship> ReadRelationships(ITable table)
    {
        HashSet<string> primaryKeyColumns = PrimaryKeyColumns(table);

        return table.ForeignKeyConstraints.Select(constraint => new SchemaRelationship(
            constraint.Name,
            PrincipalTable: constraint.PrincipalTable.Name,
            DependentTable: table.Name,
            DependentColumns: constraint.Columns.Select(c => c.Name).ToList(),
            IsRequired: constraint.Columns.All(c => !c.IsNullable),
            IsOneToOne: constraint.MappedForeignKeys.Any(fk => fk.IsUnique),
            IsIdentifying: constraint.Columns.All(c => primaryKeyColumns.Contains(c.Name))
        ));
    }

    private static HashSet<string> PrimaryKeyColumns(ITable table)
    {
        return table.PrimaryKey?.Columns.Select(c => c.Name).ToHashSet(StringComparer.Ordinal)
            ?? [];
    }

    private static int CompareRelationships(SchemaRelationship a, SchemaRelationship b)
    {
        int result = string.CompareOrdinal(a.PrincipalTable, b.PrincipalTable);
        if (result == 0)
        {
            result = string.CompareOrdinal(a.DependentTable, b.DependentTable);
        }

        return result == 0 ? string.CompareOrdinal(a.ConstraintName, b.ConstraintName) : result;
    }
}
