namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Stores a <see cref="FormControl.TextList"/> in the child rows of a collection shared with
/// other lists, told apart by an enum column, such as the generic names among a product's names
/// and identifiers. Put it, with the question's other attributes, on a property EF does not map.
/// The list reads, writes and counts only the rows holding <see cref="Kind"/>, and new rows are
/// given it.
/// </summary>
/// <param name="navigation">
/// The collection navigation on the same entity that holds the rows, for example
/// <c>nameof(NamesAndIdentifiers)</c>.
/// </param>
/// <param name="kind">
/// The enum member that marks this list's rows. The child entity must have exactly one column of
/// its enum type.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormListSourceAttribute(string navigation, object kind) : Attribute
{
    /// <summary>The collection navigation that holds the rows.</summary>
    public string Navigation { get; } = navigation;

    /// <summary>The enum member that marks this list's rows.</summary>
    public object Kind { get; } = kind;
}
