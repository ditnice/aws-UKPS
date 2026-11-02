namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>At most <see cref="Max"/> values may be chosen.</summary>
/// <param name="max">
/// The most values allowed. Must be positive. Only a multi-value question (a flags enum or a
/// junction) may have it. A multi-value <see cref="FormControl.HierarchicalSelect"/> also hides
/// its add another button once this many entries are shown.
/// </param>
/// <param name="message">
/// The error shown against the question when too many values are chosen, for example
/// <c>"Select up to 3 therapeutic areas"</c>.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormMaxItemsAttribute(int max, string message) : Attribute
{
    /// <summary>The most values allowed.</summary>
    public int Max { get; } = max;

    /// <summary>The error shown when too many values are chosen.</summary>
    public string Message { get; } = message;
}
