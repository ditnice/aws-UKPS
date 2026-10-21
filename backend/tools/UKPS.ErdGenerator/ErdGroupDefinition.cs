using System.Diagnostics.CodeAnalysis;

namespace UKPS.ErdGenerator;

/// <summary>One group entry in <c>erd-groups.yml</c>; populated by YamlDotNet via reflection.</summary>
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
internal sealed class ErdGroupDefinition
{
    public string? Title { get; set; }

    public string? Colour { get; set; }

    /// <summary>Null when the YAML list is empty or omitted; normalised during validation.</summary>
    public List<string>? Tables { get; set; }
}
