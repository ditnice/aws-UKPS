namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Stores a <see cref="FormControl.TextList"/> in the child rows of a collection shared with
/// other lists, told apart by an enum column, such as the generic names among a product's names
/// and identifiers. Put it, with the question's other attributes, on a property EF does not map.
/// The list reads, writes and counts only the rows holding <see cref="Kind"/>, and new rows are
/// given it.
/// </summary>
/// <remarks>
/// Apply it as <see cref="FormListSourceAttribute{TEnum}"/>, so the kind is checked to be an enum
/// member when it is compiled. This base lets the scanner find it whatever the enum.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal abstract class FormListSourceAttribute : Attribute
{
    private protected FormListSourceAttribute(string navigation, Enum kind)
    {
        Navigation = navigation;
        Kind = kind;
    }

    /// <summary>The collection navigation that holds the rows.</summary>
    public string Navigation { get; }

    /// <summary>The enum member that marks this list's rows.</summary>
    public Enum Kind { get; }
}

/// <inheritdoc cref="FormListSourceAttribute"/>
/// <typeparam name="TEnum">The enum type of the child entity's kind column.</typeparam>
/// <param name="navigation">
/// The collection navigation on the same entity that holds the rows, for example
/// <c>nameof(NamesAndIdentifiers)</c>.
/// </param>
/// <param name="kind">
/// The enum member that marks this list's rows. The child entity must have exactly one
/// non-nullable column of type <typeparamref name="TEnum"/>.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormListSourceAttribute<TEnum>(string navigation, TEnum kind)
    : FormListSourceAttribute(navigation, kind)
    where TEnum : struct, Enum;
