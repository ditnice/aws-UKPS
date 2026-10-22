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
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormFieldAttribute(string id, string label) : Attribute
{
    private FormControl? _control;

    /// <summary>The question's ID, used as the payload, error and audit key.</summary>
    public string Id { get; } = id;

    /// <summary>The question text.</summary>
    public string Label { get; } = label;

    /// <summary>The control to render. Inferred from the property type when not set.</summary>
    /// <remarks>
    /// Must suit the binding: text or textarea for strings, radio or select for enums, radio,
    /// select or hierarchical select for reference IDs, checkbox for flags enums, and checkbox or
    /// hierarchical select for junctions. Must be set explicitly on a reference ID property.
    /// </remarks>
    public FormControl Control
    {
        get => _control ?? default;
        set => _control = value;
    }

    /// <summary>The control set explicitly, or null when it should be inferred.</summary>
    public FormControl? ExplicitControl => _control;

    /// <summary>Optional help text shown under the label.</summary>
    public string? Hint { get; set; }

    /// <summary>
    /// A rendering hint for the frontend, for example <c>combobox</c>. It does not change
    /// validation, and the frontend ignores hints it does not know.
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
    /// The button text that adds another entry to a multi-value hierarchical select, for example
    /// <c>Add another therapy area</c>. Only a <see cref="FormControl.HierarchicalSelect"/> on a
    /// junction may set it; defaults to <c>Add another</c>.
    /// </summary>
    public string? AddAnotherLabel { get; set; }
}
