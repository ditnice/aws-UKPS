namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// The input a record content question is rendered with. Part of the HTTP API contract: the
/// member names are sent to the frontend, so never rename or remove one.
/// </summary>
/// <remarks>
/// Every <see cref="FormFieldAttribute"/> names one. Startup rejects a control that does not suit
/// the property.
/// A choice control takes its options from the one provider registered for the property's enum
/// or referenced entity, unless <see cref="FormOptionsAttribute"/> names another.
/// </remarks>
public enum FormControl
{
    /// <summary>
    /// A single-line text input for a string property. May have
    /// <see cref="FormMaxLengthAttribute"/>; must not have <see cref="FormOptionsAttribute"/>.
    /// The answer is the trimmed text, or null when blank.
    /// </summary>
    Text = 0,

    /// <summary>
    /// A multi-line text input for longer answers. Only for a string property. May have <see cref="FormMaxLengthAttribute"/>; must not have
    /// <see cref="FormOptionsAttribute"/>. The answer is the trimmed text, or null when blank.
    /// </summary>
    Textarea = 1,

    /// <summary>
    /// A single choice shown as radio buttons, best for a short list. For an enum or a
    /// reference ID property. Takes an enum or flat reference provider.
    /// The answer is one option value.
    /// </summary>
    Radio = 2,

    /// <summary>
    /// A single choice shown as a dropdown, best for a long list. Allowed on an enum or a
    /// reference ID property. Takes an enum or flat reference provider, and may set
    /// <see cref="FormFieldAttribute.Placeholder"/>. The answer is one option value.
    /// </summary>
    Select = 3,

    /// <summary>
    /// Several choices shown as checkboxes. For a flags enum or a junction, and the only control
    /// a flags enum allows. May have <see cref="FormMaxItemsAttribute"/>. The answer is an array of option values.
    /// </summary>
    Checkbox = 4,

    /// <summary>
    /// A two-level choice: a parent dropdown, then an optional child dropdown. The answer is the
    /// most specific option chosen. On a junction it allows several entries (add another).
    /// </summary>
    /// <remarks>
    /// Allowed on a reference ID property or a junction whose referenced entity has an
    /// <c>int? ParentId</c>. Takes a tree provider and needs
    /// <see cref="FormFieldAttribute.ChildLabel"/>; may set
    /// <see cref="FormFieldAttribute.Placeholder"/> and
    /// <see cref="FormFieldAttribute.ChildPlaceholder"/>. On a junction it may also set
    /// <see cref="FormFieldAttribute.AddAnotherLabel"/> and have
    /// <see cref="FormMaxItemsAttribute"/>, which caps the number of entries.
    /// </remarks>
    HierarchicalSelect = 5,

    /// <summary>
    ///  A date at a chosen precision, for a foreign key to a regulatory date: an estimated quarter
    /// and year, an estimated month and year, or an actual day, month and year. The answer is
    /// <c>yyyy-Qn</c>, <c>yyyy-MM</c> or <c>yyyy-MM-dd</c>, and its precision can trigger
    /// conditions. Needs <see cref="FormDateEventAttribute"/>.
    /// </summary>
    PrecisionDate = 6,

    /// <summary>
    /// The actual month and year of an event that has happened, for a foreign key to a regulatory
    /// date. The answer is <c>yyyy-MM</c>, this month or earlier. Needs
    /// <see cref="FormDateEventAttribute"/>.
    /// </summary>
    ActualMonth = 7,

    /// <summary>
    /// A dropdown of fixed prefixes beside a text box for the number after it, for a string
    /// property. The answer is the prefix and digits joined, for example <c>GID-TA10234</c>, or
    /// null when blank. Needs <see cref="FormValuePrefixesAttribute"/>; must not have
    /// <see cref="FormOptionsAttribute"/>.
    /// </summary>
    PrefixedNumber = 8,
}
