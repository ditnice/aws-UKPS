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

        // Junction, regulatory date, group and text list questions read their answers through a
        // navigation; a group's text lists through a navigation of each child row.
        string[] includes = entity.Questions.SelectMany(static q => Includes(q)).ToArray();

        FormEntity formEntity = new(
            entity.ClrType,
            rowSource,
            entity.ClrType.GetProperty("RevisionId")!,
            includes
        );

        return entity.Questions.Select(q => BuildQuestion(q, formEntity, options)).ToArray();
    }

    private static IEnumerable<string> Includes(QuestionDeclaration question) =>
        question.Binding.Kind switch
        {
            BindingKind.Junction or BindingKind.TextList => [question.Property.Name],
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
            question.Property,
            question.Binding.ChildType!,
            question.Binding.ChildIdProperty!,
            question.Binding.ChildDisplayOrderProperty
        );

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

        Dictionary<string, FormPageShowWhenAttribute> pageConditions = entities
            .Where(e => e.Sections.Any(s => s.RecordType == recordType))
            .SelectMany(static e => e.PageConditions)
            .ToDictionary(static c => c.PageId, StringComparer.Ordinal);

        FormSection[] sections = declared
            .GroupBy(static x => x.Section.Id, StringComparer.Ordinal)
            .Select(group =>
            {
                FormSectionAttribute section = group.First().Section;
                FormTask[] tasks = group
                    .SelectMany(static x => x.Questions)
                    .GroupBy(static x => x.First.Task.Id, StringComparer.Ordinal)
                    .Select(task =>
                    {
                        FormTaskAttribute taskAttribute = task.First().First.Task;
                        FormPage[] pages = task.GroupBy(
                                static x => x.First.Page.Id,
                                StringComparer.Ordinal
                            )
                            .Select(page => BuildPage(page, section, taskAttribute, pageConditions))
                            .OrderBy(static p => p.Order)
                            .ToArray();
                        return new FormTask(
                            taskAttribute.Id,
                            taskAttribute.Title,
                            taskAttribute.Order,
                            pages
                        );
                    })
                    .OrderBy(static t => t.Order)
                    .ToArray();
                return new FormSection(section.Id, section.Title, section.Order, tasks);
            })
            .OrderBy(static s => s.Order)
            .ToArray();

        return new FormModel(recordType, FormVersions.For(recordType)!, sections);
    }

    private static FormPage BuildPage(
        IGrouping<string, (QuestionDeclaration First, FormQuestion Second)> page,
        FormSectionAttribute section,
        FormTaskAttribute task,
        Dictionary<string, FormPageShowWhenAttribute> pageConditions
    ) =>
        new(
            page.Key,
            page.First().First.Page.Title,
            page.First().First.Page.Order,
            section.Id,
            section.Title,
            task.Id,
            task.Title,
            page.Select(static x => x.Second).OrderBy(static q => q.Order).ToArray(),
            CommercialInConfidence: page.First().First.Page.CommercialInConfidence
        )
        {
            ShowWhen = pageConditions.TryGetValue(page.Key, out var condition)
                ? Condition(condition.QuestionId, condition.Values)
                : null,
        };
}
