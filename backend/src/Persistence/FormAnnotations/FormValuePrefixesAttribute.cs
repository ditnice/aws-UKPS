namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// The fixed prefixes a <see cref="FormControl.PrefixedNumber"/> question offers, in the order
/// shown. The stored value is a prefix followed by digits.
/// </summary>
/// <param name="prefixes">
/// At least one, each non-blank, with no duplicates. Only a <see cref="FormControl.PrefixedNumber"/>
/// question may have it, and that control requires it.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormValuePrefixesAttribute(params string[] prefixes) : Attribute
{
    /// <summary>The prefixes, in the order shown.</summary>
    public IReadOnlyList<string> Prefixes { get; } = prefixes;
}
