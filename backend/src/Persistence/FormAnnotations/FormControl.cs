namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// The input a record content question is rendered with. Part of the HTTP API contract.
/// </summary>
public enum FormControl
{
    /// <summary>A single-line text input.</summary>
    Text = 0,

    /// <summary>A multi-line text input.</summary>
    Textarea = 1,

    /// <summary>A single choice shown as radio buttons.</summary>
    Radio = 2,

    /// <summary>A single choice shown as a dropdown.</summary>
    Select = 3,

    /// <summary>Several choices shown as checkboxes.</summary>
    Checkbox = 4,

    /// <summary>
    /// A two-level choice: a parent dropdown, then an optional child dropdown. The answer is the
    /// most specific option chosen. On a junction it allows several entries (add another).
    /// </summary>
    HierarchicalSelect = 5,
}
