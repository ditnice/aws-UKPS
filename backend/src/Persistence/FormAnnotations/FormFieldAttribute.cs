namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Marks a property as a question. The ID is a public contract: it is the payload key, the
/// validation error key and the audit field path. Never change a released ID.
/// </summary>
/// <param name="id">
/// The question's ID: lowercase words joined by underscores, with an optional dotted prefix,
/// such as <c>medicines_indication_detail.indication</c> (<c>^[a-z0-9_]+(\.[a-z0-9_]+)?$</c>).
/// Must be unique within the form.
/// </param>
/// <param name="label">
/// The question text. On a page with one question it is the page heading; otherwise it labels
/// the input. Must not be blank.
/// </param>
/// <param name="control">
/// The control to render. Must suit the binding: text or textarea for strings, radio or select
/// for enums, radio, select or hierarchical select for reference IDs, checkbox for flags enums,
/// and checkbox or hierarchical select for junctions.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormFieldAttribute(string id, string label, FormControl control) : Attribute
{
    /// <summary>The question's ID, used as the payload, error and audit key.</summary>
    public string Id { get; } = id;

    /// <summary>The question text.</summary>
    public string Label { get; } = label;

    /// <summary>The control to render.</summary>
    public FormControl Control { get; } = control;

    /// <summary>Optional help text shown under the label.</summary>
    public string? Hint { get; set; }

    /// <summary>
    /// A rendering hint for the frontend, for example <c>combobox</c>. It does not change
    /// validation, and the frontend ignores hints it does not know. A text field suggests values
    /// as the user types with <c>autocomplete:&lt;source&gt;</c>, where the source names where the
    /// suggestions come from, for example <c>autocomplete:company-codes</c>.
    /// </summary>
    public string? Display { get; set; }

    /// <summary>
    /// Position of the question on a page with several questions. No two questions on a page
    /// may share an order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The empty option text of a dropdown, for example <c>Choose BNF chapter</c>. Only a
    /// <see cref="FormControl.Select"/> or <see cref="FormControl.HierarchicalSelect"/> may set
    /// it; defaults to <c>Select an option</c>.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// The label of the child dropdown of a hierarchical select, shown once a parent with
    /// children is chosen. Required on a <see cref="FormControl.HierarchicalSelect"/> and not
    /// allowed on any other control.
    /// </summary>
    public string? ChildLabel { get; set; }

    /// <summary>
    /// The empty option text of the child dropdown of a hierarchical select. Only a
    /// <see cref="FormControl.HierarchicalSelect"/> may set it; defaults to
    /// <c>Select an option</c>.
    /// </summary>
    public string? ChildPlaceholder { get; set; }

    /// <summary>
    /// The button text that adds another entry, for example <c>Add another therapy area</c>. Only
    /// a <see cref="FormControl.HierarchicalSelect"/> on a junction, a
    /// <see cref="FormControl.Group"/> or a <see cref="FormControl.TextList"/> may set it;
    /// defaults to <c>Add another</c>.
    /// </summary>
    public string? AddAnotherLabel { get; set; }

    /// <summary>
    /// The heading of each item of a <see cref="FormControl.Group"/>, numbered from 1, for example
    /// <c>Clinical trial</c> gives "Clinical trial 1". Required on a group. A
    /// <see cref="FormControl.TextList"/> may also set it as the visible label of each entry: the
    /// label alone while there is one entry, numbered from 1 once there are several. Not allowed
    /// on any other control.
    /// </summary>
    public string? ItemLabel { get; set; }

    /// <summary>
    /// The button text that removes an item of a <see cref="FormControl.Group"/> or an entry of a
    /// <see cref="FormControl.TextList"/>, for example <c>Remove clinical trial</c>. Defaults to
    /// <c>Remove</c>.
    /// </summary>
    public string? RemoveLabel { get; set; }

    /// <summary>
    /// The ID of the child question whose answer identifies each item of a
    /// <see cref="FormControl.Group"/> in the field audit, for example
    /// <c>clinical_trials_gov_number</c>. The child must be required and distinct. When not set,
    /// the audit lists the items in order.
    /// </summary>
    public string? ItemKey { get; set; }
}
