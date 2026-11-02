namespace UKPS.Api.Application.RecordForms.Options;

/// <summary>
/// An option a choice question offers. The value is always a string on the wire: an enum member
/// name or a reference ID. Only hierarchical options have children.
/// </summary>
internal sealed record FormOption(string Value, string Label, IReadOnlyList<FormOption> Children)
{
    public FormOption(string value, string label)
        : this(value, label, []) { }

    /// <summary>This option and its children.</summary>
    public IEnumerable<FormOption> SelfAndChildren() => [this, .. Children];
}
