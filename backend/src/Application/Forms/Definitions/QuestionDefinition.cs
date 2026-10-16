using UKPS.Api.Application.Forms.Bindings;
using UKPS.Api.Application.Forms.Options;
using UKPS.Api.Application.Forms.Rules;

namespace UKPS.Api.Application.Forms.Definitions;

/// <summary>A question on a page and how its answer is stored.</summary>
/// <param name="Id">
/// Stable public ID (<c>table.field</c> in snake_case, or the table name for collections).
/// Used as payload key, error key and audit field path. Never change a released ID.
/// </param>
/// <param name="Type">The kind of input.</param>
/// <param name="Label">The question text.</param>
/// <param name="Hint">Optional hint text; blank lines separate paragraphs.</param>
/// <param name="Display">
/// Optional rendering hint for the client (e.g. <c>combobox</c>). Ignored by the server.
/// </param>
/// <param name="Rules">Validation rules, run on the client and the server.</param>
/// <param name="Options">Selectable options, for question types that have them.</param>
/// <param name="Binding">Where the answer is stored.</param>
internal sealed record QuestionDefinition(
    string Id,
    QuestionType Type,
    string Label,
    string? Hint,
    string? Display,
    IReadOnlyList<QuestionRule> Rules,
    QuestionOptions? Options,
    IQuestionBinding Binding
);
