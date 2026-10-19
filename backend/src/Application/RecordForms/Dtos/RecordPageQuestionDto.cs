using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A question on a page, with everything needed to render and validate it.</summary>
public sealed record RecordPageQuestionDto
{
    /// <summary>The question ID: the answer key, the error key and the form field name.</summary>
    public required string Id { get; init; }

    /// <summary>The control to render.</summary>
    public required FormControl Type { get; init; }

    /// <summary>True when the answer is an array of strings rather than a string or null.</summary>
    public required bool Multiple { get; init; }

    /// <summary>The question text. On a single-question page it is the page heading.</summary>
    public required string Label { get; init; }

    /// <summary>Hint text shown above the input; a blank line starts a new paragraph.</summary>
    public required string? Hint { get; init; }

    /// <summary>A rendering hint, for example <c>combobox</c>.</summary>
    public required string? Display { get; init; }

    /// <summary>The empty option text of a dropdown.</summary>
    public required string? Placeholder { get; init; }

    /// <summary>The heading of the child dropdown of a hierarchical select.</summary>
    public required string? ChildLabel { get; init; }

    /// <summary>The empty option text of the child dropdown of a hierarchical select.</summary>
    public required string? ChildPlaceholder { get; init; }

    /// <summary>The button text that adds another entry to a multi-value hierarchical select.</summary>
    public required string? AddAnotherLabel { get; init; }

    /// <summary>The rules, in evaluation order.</summary>
    public required IReadOnlyList<RecordPageRuleDto> Rules { get; init; }

    /// <summary>The options of a choice question, or null for a text question.</summary>
    public required IReadOnlyList<RecordPageOptionDto>? Options { get; init; }
}
