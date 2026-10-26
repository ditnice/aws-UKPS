using UKPS.Api.Application.RecordForms.Bindings;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Builds the immutable form models from declarations that passed validation.</summary>
internal static class FormModelBuilder
{
    public static Dictionary<RecordType, FormModel> Build(
        IReadOnlyList<EntityDeclaration> entities,
        OptionProviderRegistry options
    )
    {
        Dictionary<EntityDeclaration, FormQuestion[]> questions = entities.ToDictionary(
            static e => e,
            e => BuildQuestions(e, options)
        );

        return entities
            .SelectMany(static e => e.Sections)
            .Select(static s => s.RecordType)
            .Distinct()
            .ToDictionary(static t => t, t => BuildForm(t, entities, questions));
    }

    private static FormQuestion[] BuildQuestions(
        EntityDeclaration entity,
        OptionProviderRegistry options
    )
    {
        // Every section of an entity shares its row source; validation rejects disagreement only
        // where it matters (FirstSave with required columns), so take the strictest.
        FormRowSource rowSource = entity.Sections.Any(static s =>
            s.RowSource == FormRowSource.RecordCreation
        )
            ? FormRowSource.RecordCreation
            : FormRowSource.FirstSave;

        string[] includes = entity
            .Questions.Where(static q => q.Binding.Kind == BindingKind.Junction)
            .Select(static q => q.Property.Name)
            .ToArray();

        FormEntity formEntity = new(
            entity.ClrType,
            rowSource,
            entity.ClrType.GetProperty("RevisionId")!,
            includes
        );

        return entity.Questions.Select(q => BuildQuestion(q, formEntity, options)).ToArray();
    }

    private static FormQuestion BuildQuestion(
        QuestionDeclaration question,
        FormEntity entity,
        OptionProviderRegistry options
    )
    {
        FormFieldAttribute field = question.Field;
        List<FormRule> rules = [];
        if (question.Required is { } required)
        {
            rules.Add(new FormRule(RuleKind.Required, null, required.Message));
        }

        if (question.MaxLength is { } maxLength)
        {
            rules.Add(new FormRule(RuleKind.MaxLength, maxLength.Max, maxLength.Message));
        }

        if (question.MaxItems is { } maxItems)
        {
            rules.Add(new FormRule(RuleKind.MaxItems, maxItems.Max, maxItems.Message));
        }

        return new FormQuestion
        {
            Id = field.Id,
            Control = question.Control,
            Label = field.Label,
            Hint = field.Hint,
            Display = field.Display,
            Order = field.Order,
            Placeholder = field.Placeholder,
            ChildLabel = field.ChildLabel,
            ChildPlaceholder = field.ChildPlaceholder,
            AddAnotherLabel = field.AddAnotherLabel,
            Rules = [.. rules.OrderBy(static r => r.Kind)],
            OptionKey = OptionKey(question, options),
            Entity = entity,
            Binding = BuildBinding(question),
        };
    }

    /// <summary>
    /// The key named by <c>[FormOptions]</c>, or else the only provider of the expected kind and
    /// source type. Null for text controls.
    /// </summary>
    private static string? OptionKey(QuestionDeclaration question, OptionProviderRegistry options)
    {
        FormControl control = question.Control;
        if (!ControlRules.IsChoice(control))
        {
            return null;
        }

        if (question.Options is { } explicitOptions)
        {
            return explicitOptions.Key;
        }

        (OptionProviderKind kind, Type? source) = ControlRules.ExpectedProvider(
            question.Binding,
            control
        );
        return options.FindBySource(kind, source!).Single().Key;
    }

    private static IFormBinding BuildBinding(QuestionDeclaration question) =>
        question.Binding.Kind switch
        {
            BindingKind.Text => new ColumnBinding(question.Property, ColumnValueKind.Text),
            BindingKind.Enum => new ColumnBinding(question.Property, ColumnValueKind.Enum),
            BindingKind.ReferenceId => new ColumnBinding(
                question.Property,
                ColumnValueKind.ReferenceId
            ),
            BindingKind.Flags => new FlagsBinding(question.Property),
            BindingKind.Junction => new JunctionBinding(
                question.Property,
                question.Binding.JunctionType!,
                question.Binding.JunctionReferenceIdProperty!
            ),
            _ => throw new InvalidOperationException(
                $"Unsupported binding kind {question.Binding.Kind}."
            ),
        };

    private static FormModel BuildForm(
        RecordType recordType,
        IReadOnlyList<EntityDeclaration> entities,
        Dictionary<EntityDeclaration, FormQuestion[]> questions
    )
    {
        var declared = entities
            .SelectMany(e =>
                e.Sections.Where(s => s.RecordType == recordType)
                    .Select(s => (Section: s, Questions: e.Questions.Zip(questions[e])))
            )
            .ToArray();

        FormSection[] sections = declared
            .GroupBy(static x => x.Section.Id, StringComparer.Ordinal)
            .Select(group =>
            {
                FormSectionAttribute section = group.First().Section;
                FormPage[] pages = group
                    .SelectMany(static x => x.Questions)
                    .GroupBy(static x => x.First.Page.Id, StringComparer.Ordinal)
                    .Select(page => new FormPage(
                        page.Key,
                        page.First().First.Page.Title,
                        page.First().First.Page.Order,
                        section.Id,
                        section.Title,
                        page.Select(static x => x.Second).OrderBy(static q => q.Order).ToArray()
                    ))
                    .OrderBy(static p => p.Order)
                    .ToArray();
                return new FormSection(section.Id, section.Title, section.Order, pages);
            })
            .OrderBy(static s => s.Order)
            .ToArray();

        return new FormModel(recordType, FormVersions.For(recordType)!, sections);
    }
}
