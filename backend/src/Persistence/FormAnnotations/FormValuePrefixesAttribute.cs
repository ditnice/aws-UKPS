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

    /// <summary>
    /// The exact number of digits after the prefix, for example 8 for a ClinicalTrials.gov
    /// number. When 0 (the default), up to 5 digits are allowed.
    /// </summary>
    public int Digits { get; set; }

    /// <summary>
    /// What the number is, as its error messages name it at the start of a sentence, for example
    /// "Trial number must be 8 digits long". Defaults to <c>ID</c>.
    /// </summary>
    public string Name { get; set; } = "ID";
}
