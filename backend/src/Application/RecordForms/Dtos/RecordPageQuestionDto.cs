using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A question on a page, with everything needed to render and validate it.</summary>
public sealed record RecordPageQuestionDto
{
    /// <summary>The question ID: the answer key, the error key and the form field name.</summary>
    public required string Id { get; init; }

    /// <summary>The control to render.</summary>
    public required FormControl Type { get; init; }

    /// <summary>
    /// True when the answer is an array of strings rather than a string or null, as for a
    /// checkbox, a multi-value hierarchical select or a text list. False for a group, whose answer
    /// is an array of objects.
    /// </summary>
    public required bool Multiple { get; init; }

    /// <summary>The question text. On a single-question page it is the page heading.</summary>
    public required string Label { get; init; }

    /// <summary>Hint text shown above the input; a blank line starts a new paragraph.</summary>
    public required string? Hint { get; init; }

    /// <summary>A rendering hint, for example <c>combobox</c>.</summary>
    public required string? Display { get; init; }

    /// <summary>
    /// The width of a <c>Text</c> input, or of each entry of a <c>TextList</c>, overriding the
    /// default of two-thirds; null for the default. Not <c>required</c>, so existing clients and
    /// fixtures are unaffected.
    /// </summary>
    public FormInputWidth? Width { get; init; }

    /// <summary>The empty option text of a dropdown.</summary>
    public required string? Placeholder { get; init; }

    /// <summary>The heading of the child dropdown of a hierarchical select.</summary>
    public required string? ChildLabel { get; init; }

    /// <summary>The empty option text of the child dropdown of a hierarchical select.</summary>
    public required string? ChildPlaceholder { get; init; }

    /// <summary>
    /// The button text that adds another entry to a multi-value hierarchical select, a group or a
    /// text list.
    /// </summary>
    public required string? AddAnotherLabel { get; init; }

    /// <summary>
    /// The heading of each item of a group, numbered from 1, or the label of each entry of a text
    /// list, numbered from 1 once there are several; null for other controls.
    /// </summary>
    public string? ItemLabel { get; init; }

    /// <summary>The button text that removes an item of a group or an entry of a text list.</summary>
    public string? RemoveLabel { get; init; }

    /// <summary>
    /// The questions asked of each item of a group, in order, or null for other controls. Each
    /// item of the answer is an object keyed by these questions' IDs, and their errors are keyed
    /// <c>{group id}[{item index}].{child id}</c>.
    /// </summary>
    public IReadOnlyList<RecordPageQuestionDto>? Children { get; init; }

    /// <summary>
    /// The prefixes a <c>PrefixedNumber</c> question offers before its digits, in order, or
    /// null for any other control. Not <c>required</c>, so existing clients and fixtures are
    /// unaffected.
    /// </summary>
    public IReadOnlyList<string>? Prefixes { get; init; }

    /// <summary>
    /// The exact number of digits a <c>PrefixedNumber</c> needs after its prefix, or null when up
    /// to 5 are allowed.
    /// </summary>
    public int? PrefixDigits { get; init; }

    /// <summary>
    /// What a <see cref="FormControl.PrecisionDate"/> question's date is of, for its error
    /// messages, for example <c>submission</c>; null for other questions.
    /// </summary>
    public required string? DateName { get; init; }

    /// <summary>
    /// When set, the question applies only while the condition is met; otherwise it is hidden,
    /// not validated, and should be sent as null (or an empty array).
    /// </summary>
    public required RecordPageConditionDto? ShowWhen { get; init; }

    /// <summary>The rules, in evaluation order.</summary>
    public required IReadOnlyList<RecordPageRuleDto> Rules { get; init; }

    /// <summary>The options of a choice question, or null for a text question.</summary>
    public required IReadOnlyList<RecordPageOptionDto>? Options { get; init; }
}
