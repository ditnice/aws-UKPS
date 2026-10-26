namespace UKPS.ErdGenerator;

internal sealed record SchemaColumn(
    string Name,
    string StoreType,
    bool IsNullable,
    bool IsPrimaryKey,
    bool IsForeignKey,
    bool IsUnique
);
