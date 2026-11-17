using System.Reflection;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>An annotated property, as read by the scanner, before validation.</summary>
internal sealed record QuestionDeclaration
{
    public required Type EntityType { get; init; }
    public required PropertyInfo Property { get; init; }
    public required FormTaskAttribute Task { get; init; }
    public required FormPageAttribute Page { get; init; }
    public required FormFieldAttribute Field { get; init; }
    public FormRequiredAttribute? Required { get; init; }
    public FormMaxLengthAttribute? MaxLength { get; init; }
    public FormValuePrefixesAttribute? Prefixes { get; init; }
    public FormMaxItemsAttribute? MaxItems { get; init; }
    public FormOptionsAttribute? Options { get; init; }
    public FormShowWhenAttribute? ShowWhen { get; init; }
    public FormDateEventAttribute? DateEvent { get; init; }
    public FormDistinctAttribute? Distinct { get; init; }
    public required BindingInfo Binding { get; init; }

    /// <summary>
    /// For a group, the questions asked of each item, declared on the child entity. Their task
    /// and page are the group's.
    /// </summary>
    public IReadOnlyList<QuestionDeclaration> Children { get; init; } = [];

    /// <summary>True for a question asked of each item of a group.</summary>
    public bool IsGroupChild { get; init; }

    public string Where => $"{EntityType.Name}.{Property.Name}";

    public AnswerShape Shape =>
        Binding.Kind switch
        {
            BindingKind.Flags or BindingKind.Junction => AnswerShape.Multi,
            BindingKind.TextList => AnswerShape.List,
            BindingKind.Group => AnswerShape.Group,
            _ => AnswerShape.Single,
        };

    public FormControl Control => Field.Control;
}
