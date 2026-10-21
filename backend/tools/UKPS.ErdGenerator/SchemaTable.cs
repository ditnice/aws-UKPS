namespace UKPS.ErdGenerator;

internal sealed record SchemaTable(string Name, IReadOnlyList<SchemaColumn> Columns);
