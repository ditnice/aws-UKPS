using UKPS.Api.Application.Forms.Definitions;

namespace UKPS.Api.Application.Forms.Dtos;

/// <summary>A question on a record content form page.</summary>
public sealed record FormQuestionDto
{
    /// <summary>
    /// The question's stable identifier, used as its key in answers, save payloads and
    /// validation errors.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>The kind of input to render.</summary>
    public required QuestionType Type { get; init; }

    /// <summary>The question text.</summary>
    public required string Label { get; init; }

    /// <summary>Optional hint text. Blank lines separate paragraphs.</summary>
    public string? Hint { get; init; }

    /// <summary>
    /// Optional rendering hint, e.g. <c>combobox</c> for a searchable multi-select.
    /// </summary>
    public string? Display { get; init; }

    /// <summary>Validation rules to apply before saving.</summary>
    public required IReadOnlyList<FormRuleDto> Rules { get; init; }

    /// <summary>
    /// The options to choose from, for radio, checkbox and select questions; otherwise <c>null</c>.
    /// </summary>
    public IReadOnlyList<FormOptionDto>? Options { get; init; }
}
