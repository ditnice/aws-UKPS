using System.Diagnostics.CodeAnalysis;

namespace UKPS.ErdGenerator;

/// <summary>Deserialised shape of <c>erd-groups.yml</c>; populated by YamlDotNet via reflection.</summary>
[SuppressMessage(
    "Usage",
    "CA2227:Collection properties should be read only",
    Justification = "Mutable DTO populated by YamlDotNet."
)]
[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Instantiated by YamlDotNet."
)]
internal sealed class ErdGroupsFile
{
    public string? Direction { get; set; }

    /// <summary>Null when <c>groups:</c> is empty or omitted; normalised during validation.</summary>
    public Dictionary<string, ErdGroupDefinition?>? Groups { get; set; }
}
