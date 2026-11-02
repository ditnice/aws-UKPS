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
    public FormMaxItemsAttribute? MaxItems { get; init; }
    public FormOptionsAttribute? Options { get; init; }
    public required BindingInfo Binding { get; init; }

    public string Where => $"{EntityType.Name}.{Property.Name}";

    public AnswerShape Shape =>
        Binding.Kind is BindingKind.Flags or BindingKind.Junction
            ? AnswerShape.Multi
            : AnswerShape.Single;

    public FormControl Control => Field.Control;
}
