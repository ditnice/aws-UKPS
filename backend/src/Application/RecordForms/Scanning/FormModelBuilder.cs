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
        PlacedQuestion[] questions = declarations
            .Entities.SelectMany(e =>
                e.Questions.Zip(
                    BuildQuestions(e, options),
                    (declaration, question) =>
                        new PlacedQuestion(
                            e,
                            declaration,
                            question,
                            declarations.Place(declaration)!
                        )
                )
            )
            .ToArray();

        return declarations
            .Layouts.Where(layout =>
                questions.Any(q => q.Placement.RecordType == layout.RecordType)
            )
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
        // Junction and regulatory date questions read their answers through a navigation.
        string[] includes = entity
            .Questions.Select(static q =>
                q.Binding.Kind switch
                {
                    BindingKind.Junction => q.Property.Name,
                    BindingKind.RegulatoryDate => q.Binding.DateNavigation!.Name,
                    _ => null,
                }
            )
            .OfType<string>()
            .ToArray();

        FormEntity formEntity = new(
            entity.ClrType,
            entity.RowSource,
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
            Prefixes = question.Prefixes?.Prefixes,
            DateName = question.DateEvent?.Name,
            Rules = [.. rules.OrderBy(static r => r.Kind)],
            ShowWhen = question.ShowWhen is { } showWhen
                ? Condition(showWhen.QuestionId, showWhen.Values)
                : null,
            OptionKey = OptionKey(question, options),
            ReferenceType = question.Binding.ReferenceType,
            Entity = entity,
            Binding = BuildBinding(question, entity),
        };
    }

    /// <summary>
    /// A condition on enum values, stored as their member names, or on reference data codes.
    /// </summary>
    private static AnswerInCondition Condition(string questionId, IReadOnlyList<object> values) =>
        new(
            questionId,
            values
                .Select(static v =>
                    v is Enum member ? Enum.GetName(v.GetType(), member)! : (string)v
                )
                .ToArray()
        );

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

    private static IFormBinding BuildBinding(QuestionDeclaration question, FormEntity entity) =>
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
            _ => throw new InvalidOperationException(
                $"Unsupported binding kind {question.Binding.Kind}."
            ),
        };

    private static FormModel BuildForm(FormLayout layout, PlacedQuestion[] allQuestions)
    {
        PlacedQuestion[] questions = allQuestions
            .Where(q => q.Placement.RecordType == layout.RecordType)
            .ToArray();

        Dictionary<string, FormPageShowWhenAttribute> pageConditions = questions
            .Select(static q => q.Entity)
            .Distinct()
            .SelectMany(static e => e.PageConditions)
            .ToDictionary(static c => c.PageId, StringComparer.Ordinal);

        // Tasks and sections with no pages yet are left out of the form.
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
                    .ToArray()
            ))
            .Where(static s => s.Tasks.Count > 0)
            .ToArray();

        return new FormModel(layout.RecordType, FormVersions.For(layout.RecordType)!, sections);
    }

    private static FormTask BuildTask(
        FormLayoutSection section,
        FormLayoutTask task,
        PlacedQuestion[] questions,
        Dictionary<string, FormPageShowWhenAttribute> pageConditions
    ) =>
        new(
            task.Id,
            task.Title,
            task.Order,
            questions
                .Where(q => string.Equals(q.Placement.Task.Id, task.Id, StringComparison.Ordinal))
                .GroupBy(static q => q.Declaration.Page.Id, StringComparer.Ordinal)
                .Select(page => BuildPage(page, section, task, pageConditions))
                .OrderBy(static p => p.Order)
                .ToArray()
        );

    private static FormPage BuildPage(
        IGrouping<string, PlacedQuestion> page,
        FormLayoutSection section,
        FormLayoutTask task,
        Dictionary<string, FormPageShowWhenAttribute> pageConditions
    )
    {
        FormPageAttribute declared = page.First().Declaration.Page;
        return new(
            page.Key,
            declared.Title,
            declared.Order,
            section.Id,
            section.Title,
            task.Id,
            task.Title,
            page.Select(static q => q.Question).OrderBy(static q => q.Order).ToArray(),
            CommercialInConfidence: declared.CommercialInConfidence
        )
        {
            ShowWhen = pageConditions.TryGetValue(page.Key, out var condition)
                ? Condition(condition.QuestionId, condition.Values)
                : null,
        };
    }

    /// <summary>A built question with its declaration and where its page's task puts it.</summary>
    private sealed record PlacedQuestion(
        EntityDeclaration Entity,
        QuestionDeclaration Declaration,
        FormQuestion Question,
        TaskPlacement Placement
    );
}
