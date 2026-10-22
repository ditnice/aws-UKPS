namespace UKPS.ErdGenerator;

/// <summary>A validated logical group of tables.</summary>
internal sealed record ErdGroup(
    string Key,
    string Title,
    string Colour,
    IReadOnlyList<string> Tables
);
