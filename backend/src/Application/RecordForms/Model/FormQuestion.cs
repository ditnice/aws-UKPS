using UKPS.Api.Application.RecordForms.Bindings;
using UKPS.Api.Application.RecordForms.Conditions;
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
    public FormInputWidth? Width { get; init; }
    public int Order { get; init; }
    public string? Placeholder { get; init; }
    public string? ChildLabel { get; init; }
    public string? ChildPlaceholder { get; init; }
    public string? AddAnotherLabel { get; init; }

    /// <summary>
    /// The heading of each item of a group, numbered from 1, or the label of each entry of a text
    /// list, numbered from 1 once there are several.
    /// </summary>
    public string? ItemLabel { get; init; }

    /// <summary>The button text that removes an item of a group or an entry of a text list.</summary>
    public string? RemoveLabel { get; init; }

    /// <summary>The child question whose answer identifies a group item in the field audit.</summary>
    public string? ItemKey { get; init; }

    /// <summary>
    /// The questions asked of each item of a <see cref="FormControl.Group"/>, in order; empty for
    /// other controls. Their IDs are local to the group.
    /// </summary>
    public IReadOnlyList<FormQuestion> Children { get; init; } = [];

    /// <summary>The prefixes a <see cref="FormControl.PrefixedNumber"/> offers, in order.</summary>
    public IReadOnlyList<string>? Prefixes { get; init; }

    /// <summary>
    /// The exact number of digits a <see cref="FormControl.PrefixedNumber"/> needs after its
    /// prefix, or null when up to 5 are allowed.
    /// </summary>
    public int? PrefixDigits { get; init; }

    /// <summary>What a date question's date is of, as its error messages name it.</summary>
    public string? DateName { get; init; }

    /// <summary>The rules, in evaluation order.</summary>
    public required IReadOnlyList<FormRule> Rules { get; init; }

    /// <summary>When set, the question applies only while it is met.</summary>
    public FormCondition? ShowWhen { get; init; }

    public string? OptionKey { get; init; }

    /// <summary>The reference data entity of a reference ID or junction question.</summary>
    public Type? ReferenceType { get; init; }

    /// <summary>
    /// The content entity whose row holds the answer. For a group child, the entity holding the
    /// group, since the child is read and written only through the group.
    /// </summary>
    public required FormEntity Entity { get; init; }
    public required IFormBinding Binding { get; init; }

    public bool IsRequired => Rules.Any(static rule => rule.Kind == RuleKind.Required);

    public AnswerShape Shape => Binding.Shape;

    public bool HasOptions => OptionKey is not null;
}
