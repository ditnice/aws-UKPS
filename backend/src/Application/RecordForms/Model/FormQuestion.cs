using UKPS.Api.Application.RecordForms.Bindings;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>A question on a page, and the binding that stores its answer.</summary>
internal sealed record FormQuestion
{
    public required string Id { get; init; }
    public required FormControl Control { get; init; }
    public required string Label { get; init; }
    public string? Hint { get; init; }
    public string? Display { get; init; }
    public int Order { get; init; }
    public string? Placeholder { get; init; }
    public string? ChildLabel { get; init; }
    public string? ChildPlaceholder { get; init; }
    public string? AddAnotherLabel { get; init; }

    /// <summary>The rules, in evaluation order.</summary>
    public required IReadOnlyList<FormRule> Rules { get; init; }

    public string? OptionKey { get; init; }
    public required FormEntity Entity { get; init; }
    public required IFormBinding Binding { get; init; }

    public bool IsRequired => Rules.Any(static rule => rule.Kind == RuleKind.Required);

    public AnswerShape Shape => Binding.Shape;

    public bool HasOptions => OptionKey is not null;
}
