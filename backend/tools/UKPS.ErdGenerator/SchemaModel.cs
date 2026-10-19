namespace UKPS.ErdGenerator;

/// <summary>A provider-neutral snapshot of the relational schema derived from the EF Core model.</summary>
internal sealed record SchemaModel(
    IReadOnlyList<SchemaTable> Tables,
    IReadOnlyList<SchemaRelationship> Relationships
);
