namespace UKPS.ErdGenerator;

/// <summary>
/// A foreign key constraint, viewed from the dependent (child) table towards the
/// principal (parent) table.
/// </summary>
internal sealed record SchemaRelationship(
    string ConstraintName,
    string PrincipalTable,
    string DependentTable,
    IReadOnlyList<string> DependentColumns,
    bool IsRequired,
    bool IsOneToOne,
    bool IsIdentifying
);
