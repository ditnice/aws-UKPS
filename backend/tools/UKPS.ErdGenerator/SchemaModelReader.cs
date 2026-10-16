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

        HashSet<string> uniqueColumns = UniqueColumnSets(table)
            .Where(set => set.Count == 1)
            .Select(set => set.Single())
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
        List<HashSet<string>> uniqueColumnSets = UniqueColumnSets(table);

        return table.ForeignKeyConstraints.Select(constraint => new SchemaRelationship(
            constraint.Name,
            PrincipalTable: constraint.PrincipalTable.Name,
            DependentTable: table.Name,
            DependentColumns: constraint.Columns.Select(c => c.Name).ToList(),
            IsRequired: constraint.Columns.All(c => !c.IsNullable),
            IsOneToOne: IsOneToOne(constraint, uniqueColumnSets),
            IsIdentifying: constraint.Columns.All(c => primaryKeyColumns.Contains(c.Name))
        ));
    }

    /// <summary>
    /// A foreign key is one-to-one when EF models it as unique, or when the database
    /// enforces uniqueness over exactly its columns via a unique constraint or index.
    /// The latter catches relationships configured as <c>WithMany()</c> but constrained
    /// to a single dependent row by an explicit unique index.
    /// </summary>
    private static bool IsOneToOne(
        IForeignKeyConstraint constraint,
        List<HashSet<string>> uniqueColumnSets
    )
    {
        if (constraint.MappedForeignKeys.Any(fk => fk.IsUnique))
        {
            return true;
        }

        HashSet<string> foreignKeyColumns = constraint
            .Columns.Select(c => c.Name)
            .ToHashSet(StringComparer.Ordinal);

        return uniqueColumnSets.Any(set => set.SetEquals(foreignKeyColumns));
    }

    /// <summary>
    /// Column sets over which the database enforces global uniqueness: non-primary-key unique
    /// constraints and unfiltered unique indexes. Filtered (partial) unique indexes are
    /// excluded because they only constrain a subset of rows and would otherwise misrepresent
    /// a column as unique across the whole table.
    /// </summary>
    private static List<HashSet<string>> UniqueColumnSets(ITable table)
    {
        IEnumerable<IReadOnlyList<IColumn>> constraintColumns = table
            .UniqueConstraints.Where(uc => !uc.GetIsPrimaryKey())
            .Select(uc => uc.Columns);

        IEnumerable<IReadOnlyList<IColumn>> indexColumns = table
            .Indexes.Where(ix => ix.IsUnique && string.IsNullOrEmpty(ix.Filter))
            .Select(ix => ix.Columns);

        return constraintColumns
            .Concat(indexColumns)
            .Select(columns => columns.Select(c => c.Name).ToHashSet(StringComparer.Ordinal))
            .ToList();
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
