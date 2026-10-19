namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Marks a property as a question. The ID is a public contract: it is the payload key, the
/// validation error key and the audit field path. Never change a released ID.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormFieldAttribute(string id, string label) : Attribute
{
    private FormControl? _control;

    public string Id { get; } = id;
    public string Label { get; } = label;

    /// <summary>The control to render. Inferred from the property type when not set.</summary>
    public FormControl Control
    {
        get => _control ?? default;
        set => _control = value;
    }

    /// <summary>The control set explicitly, or null when it should be inferred.</summary>
    public FormControl? ExplicitControl => _control;

    public string? Hint { get; set; }

    /// <summary>A rendering hint for the frontend, for example <c>combobox</c>.</summary>
    public string? Display { get; set; }

    /// <summary>Position of the question on a page with several questions.</summary>
    public int Order { get; set; }

    /// <summary>The empty option text of a dropdown.</summary>
    public string? Placeholder { get; set; }

    /// <summary>The heading of the child dropdown of a hierarchical select.</summary>
    public string? ChildLabel { get; set; }

    /// <summary>The empty option text of the child dropdown of a hierarchical select.</summary>
    public string? ChildPlaceholder { get; set; }

    /// <summary>The button text that adds another entry to a multi-value hierarchical select.</summary>
    public string? AddAnotherLabel { get; set; }
}
