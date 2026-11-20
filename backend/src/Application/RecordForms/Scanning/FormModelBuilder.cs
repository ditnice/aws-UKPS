using UKPS.Api.Application.RecordForms.Bindings;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Builds the immutable form models from declarations that passed validation.</summary>
internal static class FormModelBuilder
{
    public static Dictionary<RecordType, FormModel> Build(
        FormDeclarations declarations,
        OptionProviderRegistry options
    )
    {
        Dictionary<QuestionDeclaration, FormQuestion> built = declarations
            .Entities.SelectMany(e => e.Questions.Zip(BuildQuestions(e, options)))
            .ToDictionary(static x => x.First, static x => x.Second);
        BuiltQuestion[] questions = declarations
            .Placed.Select(p => new BuiltQuestion(p, built[p.Question]))
            .ToArray();

        return declarations
            .Layouts.Where(layout => questions.Any(q => q.Placed.RecordType == layout.RecordType))
            .ToDictionary(
                static layout => layout.RecordType,
                layout => BuildForm(layout, questions)
            );
    }

    private static FormQuestion[] BuildQuestions(
        EntityDeclaration entity,
        OptionProviderRegistry options
    )
    {
        // Junction, regulatory date, group and text list questions read their answers through a
        // navigation, which text lists sharing a collection share; a group's text lists through a
        // navigation of each child row.
        string[] includes = entity
            .Questions.SelectMany(static q => Includes(q))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        FormEntity formEntity = new(
            entity.ClrType,
            entity.RowSource,
            entity.ClrType.GetProperty("RevisionId")!,
            includes
        );

        return entity.Questions.Select(q => BuildQuestion(q, formEntity, options)).ToArray();
    }

    private static IEnumerable<string> Includes(QuestionDeclaration question) =>
        question.Binding.Kind switch
        {
            BindingKind.Junction => [question.Property.Name],
            BindingKind.TextList => [question.Binding.ChildNavigation!.Name],
            BindingKind.RegulatoryDate => [question.Binding.DateNavigation!.Name],
            BindingKind.Group =>
            [
                question.Property.Name,
                .. question
                    .Children.SelectMany(static c => Includes(c))
                    .Select(include => $"{question.Property.Name}.{include}"),
            ],
            _ => [],
        };

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

        if (question.Distinct is { } distinct)
        {
            rules.Add(new FormRule(RuleKind.Distinct, null, distinct.Message));
        }

        FormQuestion[] children = question
            .Children.Select(c => BuildQuestion(c, entity, options))
            .OrderBy(static c => c.Order)
            .ToArray();

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
            ItemLabel = field.ItemLabel,
            RemoveLabel = field.RemoveLabel,
            ItemKey = field.ItemKey,
            Children = children,
            Prefixes = question.Prefixes?.Prefixes,
            PrefixDigits = question.Prefixes is { Digits: > 0 } prefixes ? prefixes.Digits : null,
            DateName = question.DateEvent?.Name,
            Rules = [.. rules.OrderBy(static r => r.Kind)],
            ShowWhen = question.ShowWhen is { } showWhen
                ? Condition(showWhen.QuestionId, showWhen.Values)
                : null,
            OptionKey = OptionKey(question, options),
            ReferenceType = question.Binding.ReferenceType,
            Entity = entity,
            Binding = BuildBinding(question, entity, children),
        };
    }

    /// <summary>
    /// A condition on enum values, stored as their member names, or on reference data codes.
    /// </summary>
    private static FormCondition Condition(
        string questionId,
        IReadOnlyList<object> values,
        bool noneOf = false
    )
    {
        string[] names = values
            .Select(static v => v is Enum member ? Enum.GetName(v.GetType(), member)! : (string)v)
            .ToArray();
        return noneOf
            ? new AnswerNotInCondition(questionId, names)
            : new AnswerInCondition(questionId, names);
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

    private static IFormBinding BuildBinding(
        QuestionDeclaration question,
        FormEntity entity,
        IReadOnlyList<FormQuestion> children
    ) =>
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
            BindingKind.RegulatoryDate => new RegulatoryDateBinding(
                question.Property,
                question.Binding.DateNavigation!,
                entity.RevisionIdProperty,
                question.DateEvent!.DateEvent,
                question.Control
            ),
            BindingKind.TextList => new TextListBinding(
                ChildCollection(question),
                question.Binding.ChildValueProperty!
            ),
            BindingKind.Group => new GroupBinding(
                ChildCollection(question),
                children,
                question.Field.ItemKey
            ),
            _ => throw new InvalidOperationException(
                $"Unsupported binding kind {question.Binding.Kind}."
            ),
        };

    private static ChildCollection ChildCollection(QuestionDeclaration question) =>
        new(
            question.Binding.ChildNavigation ?? question.Property,
            question.Binding.ChildType!,
            question.Binding.ChildIdProperty!,
            question.Binding.ChildDisplayOrderProperty,
            question.Binding.ChildKindProperty,
            question.Binding.ChildKind
        );

    private static FormModel BuildForm(FormLayout layout, BuiltQuestion[] allQuestions)
    {
        BuiltQuestion[] questions = allQuestions
            .Where(q => q.Placed.RecordType == layout.RecordType)
            .ToArray();

        Dictionary<string, FormPageShowWhenAttribute> pageConditions = questions
            .Select(static q => q.Placed.Entity)
            .Distinct()
            .SelectMany(static e => e.PageConditions)
            .ToDictionary(static c => c.PageId, StringComparer.Ordinal);

        // Pages with no questions yet, and tasks and sections with no such pages, are left out of
        // the form.
        FormSection[] sections = layout
            .Sections.OrderBy(static s => s.Order)
            .Select(section => new FormSection(
                section.Id,
                section.Title,
                section.Order,
                section
                    .Tasks.OrderBy(static t => t.Order)
                    .Select(task => BuildTask(section, task, questions, pageConditions))
                    .Where(static t => t.Pages.Count > 0)
                    .ToArray(),
                section.Optional
            ))
            .Where(static s => s.Tasks.Count > 0)
            .ToArray();

        return new FormModel(layout.RecordType, FormVersions.For(layout.RecordType)!, sections);
    }

    private static FormTask BuildTask(
        FormLayoutSection section,
        FormLayoutTask task,
        BuiltQuestion[] questions,
        Dictionary<string, FormPageShowWhenAttribute> pageConditions
    ) =>
        new(
            task.Id,
            task.Title,
            task.Order,
            task.Pages.OrderBy(static p => p.Order)
                .Select(page => BuildPage(section, task, page, questions, pageConditions))
                .Where(static p => p.Questions.Count > 0)
                .ToArray()
        );

    private static FormPage BuildPage(
        FormLayoutSection section,
        FormLayoutTask task,
        FormLayoutPage page,
        BuiltQuestion[] questions,
        Dictionary<string, FormPageShowWhenAttribute> pageConditions
    ) =>
        new(
            page.Id,
            page.Title,
            page.Order,
            section.Id,
            section.Title,
            task.Id,
            task.Title,
            questions
                .Where(q => string.Equals(q.Placed.PageId, page.Id, StringComparison.Ordinal))
                .Select(static q => q.Question)
                .OrderBy(static q => q.Order)
                .ToArray(),
            CommercialInConfidence: page.CommercialInConfidence
        )
        {
            ShowWhen = pageConditions.TryGetValue(page.Id, out var condition)
                ? Condition(condition.QuestionId, condition.Values, condition.NoneOf)
                : null,
        };

    /// <summary>A placed question declaration with the question built from it.</summary>
    private sealed record BuiltQuestion(PlacedQuestion Placed, FormQuestion Question);
}
