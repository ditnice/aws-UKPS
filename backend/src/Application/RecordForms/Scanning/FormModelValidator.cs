using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>
/// Checks scanned declarations against every startup rule and returns all the problems at once,
/// so a broken build lists everything to fix. The rules are catalogued in
/// <c>documentation/form-generation/creating-form-pages.md</c>.
/// </summary>
internal static partial class FormModelValidator
{
    private const string RevisionIdProperty = "RevisionId";

    // Page IDs are the last segment of /portal/records/{recordId}/{pageId}, so a page cannot take
    // the ID of a fixed route beside them.
    private const string TaskListRoute = "task-list";

    public static IReadOnlyList<string> Validate(
        IReadOnlyList<EntityDeclaration> entities,
        OptionProviderRegistry options
    )
    {
        List<string> problems = [];

        foreach (EntityDeclaration entity in entities)
        {
            ValidateEntity(entity, problems);
            foreach (QuestionDeclaration question in entity.Questions)
            {
                ValidateQuestion(question, options, problems);
            }
        }

        IEnumerable<RecordType> recordTypes = entities
            .SelectMany(static e => e.Sections)
            .Select(static s => s.RecordType)
            .Distinct();
        foreach (RecordType recordType in recordTypes)
        {
            ValidateForm(recordType, entities, problems);
        }

        return problems;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex SlugPattern();

    [GeneratedRegex(@"^[a-z0-9_]+(\.[a-z0-9_]+)?$", RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex QuestionIdPattern();

    private static void ValidateEntity(EntityDeclaration entity, List<string> problems)
    {
        string name = entity.ClrType.Name;

        IProperty? revisionId = entity.EntityType.FindProperty(RevisionIdProperty);
        bool hasRevisionKey =
            revisionId is not null
            && revisionId.ClrType == typeof(int)
            && revisionId
                .GetContainingForeignKeys()
                .Any(static fk => fk.PrincipalEntityType.ClrType == typeof(RecordRevision));
        if (!hasRevisionKey)
        {
            problems.Add($"{name} has no int RevisionId foreign key to RecordRevision.");
        }

        if (entity.Sections.Any(static s => s.RowSource == FormRowSource.FirstSave))
        {
            string[] required = entity
                .EntityType.GetProperties()
                .Where(static p =>
                    !p.IsNullable
                    && !p.IsPrimaryKey()
                    && !string.Equals(p.Name, RevisionIdProperty, StringComparison.Ordinal)
                    && p.GetDefaultValueSql() is null
                    && p.GetDefaultValue() is null
                )
                .Select(static p => p.Name)
                .ToArray();
            if (required.Length > 0)
            {
                problems.Add(
                    $"{name} uses RowSource FirstSave but has non-nullable columns ({string.Join(", ", required)}); use RowSource RecordCreation."
                );
            }
        }

        foreach (
            IGrouping<RecordType, FormSectionAttribute> duplicate in entity
                .Sections.GroupBy(static s => s.RecordType)
                .Where(static g => g.Count() > 1)
        )
        {
            problems.Add($"{name} declares more than one [FormSection] for {duplicate.Key}.");
        }

        foreach (FormSectionAttribute section in entity.Sections)
        {
            if (!SlugPattern().IsMatch(section.Id) || string.IsNullOrWhiteSpace(section.Title))
            {
                problems.Add(
                    $"{name} declares section '{section.Id}' with a badly formed ID or a blank title."
                );
            }
        }

        if (entity.Questions.Count == 0)
        {
            problems.Add($"{name} declares [FormSection] but has no questions.");
        }
    }

    private static void ValidateQuestion(
        QuestionDeclaration question,
        OptionProviderRegistry options,
        List<string> problems
    )
    {
        string where = question.Where;
        FormFieldAttribute field = question.Field;

        if (!QuestionIdPattern().IsMatch(field.Id))
        {
            problems.Add($"{where} has a badly formed question ID '{field.Id}'.");
        }

        if (
            !SlugPattern().IsMatch(question.Task.Id)
            || string.IsNullOrWhiteSpace(question.Task.Title)
        )
        {
            problems.Add(
                $"{where} declares task '{question.Task.Id}' with a badly formed ID or a blank title."
            );
        }

        if (
            !SlugPattern().IsMatch(question.Page.Id)
            || string.IsNullOrWhiteSpace(question.Page.Title)
        )
        {
            problems.Add(
                $"{where} declares page '{question.Page.Id}' with a badly formed ID or a blank title."
            );
        }

        if (string.Equals(question.Page.Id, TaskListRoute, StringComparison.Ordinal))
        {
            problems.Add(
                $"{where} declares page '{TaskListRoute}', which is reserved for the task list."
            );
        }

        if (string.IsNullOrWhiteSpace(field.Label))
        {
            problems.Add($"{where} has a blank label.");
        }

        FormControl control = question.Control;
        if (!ControlRules.Allowed(question.Binding.Kind).Contains(control))
        {
            problems.Add(
                $"{where} uses control {control} which does not suit a {question.Binding.Kind} binding."
            );
            return;
        }

        if (!question.Binding.IsNullable && question.Required is null)
        {
            problems.Add($"{where} is bound to a non-nullable column, so it needs [FormRequired].");
        }

        ValidateDateEvent(question, control, problems);
        ValidateOptions(question, control, options, problems);
        ValidateRules(question, control, problems);
        ValidatePresentation(question, control, problems);
    }

    private static void ValidateDateEvent(
        QuestionDeclaration question,
        FormControl control,
        List<string> problems
    )
    {
        bool isDate = question.Binding.Kind == BindingKind.RegulatoryDate;
        if (isDate && question.DateEvent is null)
        {
            problems.Add($"{question.Where} uses control {control}, which needs [FormDateEvent].");
        }
        else if (!isDate && question.DateEvent is not null)
        {
            problems.Add(
                $"{question.Where} has [FormDateEvent], which only a PrecisionDate or ActualMonth uses."
            );
        }
    }

    private static void ValidateOptions(
        QuestionDeclaration question,
        FormControl control,
        OptionProviderRegistry options,
        List<string> problems
    )
    {
        string where = question.Where;
        if (!ControlRules.IsChoice(control))
        {
            if (question.Options is not null)
            {
                problems.Add($"{where} uses text control {control} but has [FormOptions].");
            }

            return;
        }

        (OptionProviderKind expectedKind, Type? expectedSource) = ControlRules.ExpectedProvider(
            question.Binding,
            control
        );

        if (question.Options is null)
        {
            ValidateDefaultProvider(
                where,
                control,
                expectedKind,
                expectedSource,
                options,
                problems
            );
        }
        else
        {
            ValidateNamedProvider(
                where,
                question.Options.Key,
                expectedKind,
                expectedSource,
                options,
                problems
            );
        }

        if (
            control == FormControl.HierarchicalSelect
            && expectedSource?.GetProperty("ParentId")?.PropertyType != typeof(int?)
        )
        {
            problems.Add(
                $"{where} uses HierarchicalSelect but {expectedSource?.Name} has no int? ParentId."
            );
        }
    }

    /// <summary>Checks there is exactly one provider for a question with no [FormOptions].</summary>
    private static void ValidateDefaultProvider(
        string where,
        FormControl control,
        OptionProviderKind expectedKind,
        Type? expectedSource,
        OptionProviderRegistry options,
        List<string> problems
    )
    {
        IReadOnlyList<IOptionProvider> candidates = expectedSource is null
            ? []
            : options.FindBySource(expectedKind, expectedSource);
        if (candidates.Count == 0)
        {
            problems.Add(
                $"{where} uses choice control {control} but no {expectedKind} provider of {expectedSource?.Name} is registered; register one or add [FormOptions]."
            );
        }
        else if (candidates.Count > 1)
        {
            problems.Add(
                $"{where} has several {expectedKind} providers of {expectedSource?.Name} ({string.Join(", ", candidates.Select(static p => $"'{p.Key}'"))}); choose one with [FormOptions]."
            );
        }
    }

    /// <summary>Checks the provider named by [FormOptions] exists and suits the question.</summary>
    private static void ValidateNamedProvider(
        string where,
        string key,
        OptionProviderKind expectedKind,
        Type? expectedSource,
        OptionProviderRegistry options,
        List<string> problems
    )
    {
        IOptionProvider? provider = options.Find(key);
        if (provider is null)
        {
            problems.Add($"{where} uses unknown option provider '{key}'.");
        }
        else if (provider.Kind != expectedKind || provider.SourceType != expectedSource)
        {
            problems.Add(
                $"{where} needs a {expectedKind} provider of {expectedSource?.Name}, but '{provider.Key}' is a {provider.Kind} provider of {provider.SourceType.Name}."
            );
        }
    }

    private static void ValidateRules(
        QuestionDeclaration question,
        FormControl control,
        List<string> problems
    )
    {
        string where = question.Where;
        if (
            question.MaxLength is { } maxLength
            && (control is not (FormControl.Text or FormControl.Textarea) || maxLength.Max <= 0)
        )
        {
            problems.Add(
                $"{where} has [FormMaxLength], which needs a Text or Textarea control and a positive maximum."
            );
        }

        if (
            question.MaxItems is { } maxItems
            && (question.Shape != AnswerShape.Multi || maxItems.Max <= 0)
        )
        {
            problems.Add(
                $"{where} has [FormMaxItems], which needs a multi-value question and a positive maximum."
            );
        }
    }

    private static void ValidatePrefixes(
        QuestionDeclaration question,
        FormControl control,
        List<string> problems
    )
    {
        string where = question.Where;
        IReadOnlyList<string>? prefixes = question.Prefixes?.Prefixes;

        if (control != FormControl.PrefixedNumber)
        {
            if (prefixes is not null)
            {
                problems.Add($"{where} has [FormValuePrefixes], which only a PrefixedNumber uses.");
            }

            return;
        }

        if (prefixes is null or { Count: 0 })
        {
            problems.Add(
                $"{where} uses PrefixedNumber, which needs [FormValuePrefixes] with at least one prefix."
            );
            return;
        }

        if (prefixes.Any(static p => string.IsNullOrWhiteSpace(p) || char.IsAsciiDigit(p[^1])))
        {
            problems.Add(
                $"{where} has a blank prefix, or one ending in a digit, which would make the number ambiguous."
            );
        }

        if (prefixes.Distinct(StringComparer.Ordinal).Count() != prefixes.Count)
        {
            problems.Add($"{where} repeats a prefix in [FormValuePrefixes].");
        }

        if (
            prefixes.Any(a =>
                prefixes.Any(b =>
                    !string.Equals(a, b, StringComparison.Ordinal)
                    && b.StartsWith(a, StringComparison.Ordinal)
                )
            )
        )
        {
            problems.Add(
                $"{where} has a prefix that starts another, which makes the split ambiguous."
            );
        }
    }

    private static void ValidatePresentation(
        QuestionDeclaration question,
        FormControl control,
        List<string> problems
    )
    {
        ValidatePrefixes(question, control, problems);
        string where = question.Where;
        FormFieldAttribute field = question.Field;
        bool isHierarchical = control == FormControl.HierarchicalSelect;

        if (
            field.Placeholder is not null
            && control
                is not (
                    FormControl.Select
                    or FormControl.HierarchicalSelect
                    or FormControl.PrefixedNumber
                )
        )
        {
            problems.Add(
                $"{where} sets Placeholder, which only a Select or HierarchicalSelect or PrefixedNumber uses."
            );
        }

        if ((field.ChildLabel is not null || field.ChildPlaceholder is not null) && !isHierarchical)
        {
            problems.Add(
                $"{where} sets ChildLabel or ChildPlaceholder, which only a HierarchicalSelect uses."
            );
        }

        if (
            field.AddAnotherLabel is not null
            && !(isHierarchical && question.Shape == AnswerShape.Multi)
        )
        {
            problems.Add(
                $"{where} sets AddAnotherLabel, which only a multi-value HierarchicalSelect uses."
            );
        }

        if (isHierarchical && field.ChildLabel is null)
        {
            problems.Add($"{where} uses HierarchicalSelect, which needs a ChildLabel.");
        }
    }

    private static void ValidateForm(
        RecordType recordType,
        IReadOnlyList<EntityDeclaration> entities,
        List<string> problems
    )
    {
        string form = $"The {recordType} form";
        if (FormVersions.For(recordType) is null)
        {
            problems.Add($"{form} has no entry in FormVersions.");
        }

        var sections = entities
            .SelectMany(e =>
                e.Sections.Where(s => s.RecordType == recordType)
                    .Select(s => (Entity: e, Section: s))
            )
            .ToArray();

        foreach (var group in sections.GroupBy(static x => x.Section.Id, StringComparer.Ordinal))
        {
            if (group.Select(static x => (x.Section.Title, x.Section.Order)).Distinct().Count() > 1)
            {
                problems.Add(
                    $"{form} declares section '{group.Key}' with different titles or orders."
                );
            }
        }

        ReportTies(
            sections.DistinctBy(static x => x.Section.Id, StringComparer.Ordinal),
            static x => x.Section.Order,
            static x => $"section '{x.Section.Id}'",
            form,
            problems
        );

        var questions = sections
            .SelectMany(static x =>
                x.Entity.Questions.Select(q => (Question: q, SectionId: x.Section.Id))
            )
            .ToArray();
        ValidateTasks(form, questions, problems);
        ValidateConditions(form, sections, problems);

        foreach (
            var duplicate in questions
                .GroupBy(static x => x.Question.Field.Id, StringComparer.Ordinal)
                .Where(static g => g.Count() > 1)
        )
        {
            problems.Add($"{form} uses question ID '{duplicate.Key}' more than once.");
        }

        ValidateDateEvents(form, questions, problems);
    }

    /// <summary>
    /// A revision holds one regulatory date per event and precision, so two date questions in a
    /// form with the same event would overwrite each other's row or break its unique index.
    /// </summary>
    private static void ValidateDateEvents(
        string form,
        (QuestionDeclaration Question, string SectionId)[] questions,
        List<string> problems
    )
    {
        foreach (
            var duplicate in questions
                .Where(static x => x.Question.Binding.Kind == BindingKind.RegulatoryDate)
                .Where(static x => x.Question.DateEvent is not null)
                .GroupBy(static x => x.Question.DateEvent!.DateEvent)
                .Where(static g => g.Count() > 1)
        )
        {
            string ids = string.Join(
                ", ",
                duplicate.Select(static x => $"'{x.Question.Field.Id}'")
            );
            problems.Add(
                $"{form} uses date event {duplicate.Key} on more than one question: {ids}."
            );
        }
    }

    private static void ValidateTasks(
        string form,
        (QuestionDeclaration Question, string SectionId)[] questions,
        List<string> problems
    )
    {
        var tasks = questions
            .GroupBy(static x => x.Question.Task.Id, StringComparer.Ordinal)
            .ToArray();
        ValidateTaskMetadata(form, tasks, problems);
        ValidatePages(form, questions, problems);
    }

    private static void ValidateTaskMetadata(
        string form,
        IGrouping<string, (QuestionDeclaration Question, string SectionId)>[] tasks,
        List<string> problems
    )
    {
        foreach (var task in tasks)
        {
            if (
                task.Select(static x => (x.Question.Task.Title, x.Question.Task.Order, x.SectionId))
                    .Distinct()
                    .Count() > 1
            )
            {
                problems.Add(
                    $"{form} declares task '{task.Key}' with different titles, orders or sections."
                );
            }
        }

        foreach (
            var section in tasks
                .Select(static task => task.First())
                .GroupBy(static x => x.SectionId, StringComparer.Ordinal)
        )
        {
            ReportTies(
                section,
                static x => x.Question.Task.Order,
                static x => $"task '{x.Question.Task.Id}'",
                $"{form}, section '{section.Key}',",
                problems
            );
        }
    }

    private static void ValidatePages(
        string form,
        (QuestionDeclaration Question, string SectionId)[] questions,
        List<string> problems
    )
    {
        var pages = questions
            .GroupBy(
                static x => (x.SectionId, TaskId: x.Question.Task.Id, x.Question.Page.Id),
                EqualityComparer<(string SectionId, string TaskId, string Id)>.Default
            )
            .ToArray();
        ValidatePageMetadata(form, pages, problems);
        foreach (var page in pages)
        {
            ReportTies(
                page,
                static x => x.Question.Field.Order,
                static x => $"question '{x.Question.Field.Id}'",
                $"{form}, page '{page.Key.Id}',",
                problems
            );
        }

        // Grouping by task ID alone is safe: ValidateTaskMetadata reports a task ID declared in more
        // than one section, so within a valid form each task ID names exactly one task.
        foreach (
            var task in pages
                .Select(static p => p.First())
                .GroupBy(static x => x.Question.Task.Id, StringComparer.Ordinal)
        )
        {
            ReportTies(
                task,
                static x => x.Question.Page.Order,
                static x => $"page '{x.Question.Page.Id}'",
                $"{form}, task '{task.Key}',",
                problems
            );
        }

        foreach (
            var page in questions
                .GroupBy(static x => x.Question.Page.Id, StringComparer.Ordinal)
                .Where(static page =>
                    page.Select(x => x.Question.Task.Id).Distinct(StringComparer.Ordinal).Count()
                    > 1
                )
        )
        {
            problems.Add($"{form} has page '{page.Key}' in more than one task.");
        }
    }

    /// <summary>
    /// Checks every question and page condition: the trigger exists, comes earlier, and is a
    /// trigger the values suit (see <see cref="ValidateTrigger"/>).
    /// </summary>
    private static void ValidateConditions(
        string form,
        (EntityDeclaration Entity, FormSectionAttribute Section)[] sections,
        List<string> problems
    )
    {
        // Form position of each question: section order, task order, page order, then question
        // order.
        Dictionary<string, (QuestionDeclaration Question, int SectionOrder)> questions = sections
            .SelectMany(static x =>
                x.Entity.Questions.Select(q => (Question: q, SectionOrder: x.Section.Order))
            )
            .DistinctBy(static x => x.Question.Field.Id, StringComparer.Ordinal)
            .ToDictionary(static x => x.Question.Field.Id, StringComparer.Ordinal);

        ValidateQuestionConditions(questions, problems);
        ValidatePageConditions(form, sections, questions, problems);
    }

    private static void ValidateQuestionConditions(
        Dictionary<string, (QuestionDeclaration Question, int SectionOrder)> questions,
        List<string> problems
    )
    {
        foreach (var (question, _) in questions.Values)
        {
            if (question.ShowWhen is not { } showWhen)
            {
                continue;
            }

            string where = $"{question.Where} [FormShowWhen]";
            if (!question.Binding.IsNullable)
            {
                problems.Add(
                    $"{where} is on a non-nullable column, which cannot be cleared when the question does not apply."
                );
            }

            if (!questions.TryGetValue(showWhen.QuestionId, out var trigger))
            {
                problems.Add($"{where} names unknown question '{showWhen.QuestionId}'.");
                continue;
            }

            if (
                !string.Equals(trigger.Question.Page.Id, question.Page.Id, StringComparison.Ordinal)
                || trigger.Question.Field.Order >= question.Field.Order
            )
            {
                problems.Add(
                    $"{where} names '{showWhen.QuestionId}', which must be an earlier question on the same page."
                );
            }

            ValidateTrigger(where, trigger.Question, showWhen.Values, problems);
        }
    }

    private static void ValidatePageConditions(
        string form,
        (EntityDeclaration Entity, FormSectionAttribute Section)[] sections,
        Dictionary<string, (QuestionDeclaration Question, int SectionOrder)> questions,
        List<string> problems
    )
    {
        var pageConditions = sections
            .DistinctBy(static x => x.Entity)
            .SelectMany(static x => x.Entity.PageConditions.Select(c => (x.Entity, Condition: c)))
            .ToArray();

        foreach (
            var duplicate in pageConditions
                .GroupBy(static x => x.Condition.PageId, StringComparer.Ordinal)
                .Where(static g => g.Count() > 1)
        )
        {
            problems.Add(
                $"{form} has more than one [FormPageShowWhen] for page '{duplicate.Key}'."
            );
        }

        foreach (var (entity, condition) in pageConditions)
        {
            string where =
                $"{entity.ClrType.Name} [FormPageShowWhen] for page '{condition.PageId}'";
            QuestionDeclaration? onPage = entity.Questions.FirstOrDefault(q =>
                string.Equals(q.Page.Id, condition.PageId, StringComparison.Ordinal)
            );
            if (onPage is null)
            {
                problems.Add($"{where} names a page that {entity.ClrType.Name} does not declare.");
                continue;
            }

            if (!questions.TryGetValue(condition.QuestionId, out var trigger))
            {
                problems.Add($"{where} names unknown question '{condition.QuestionId}'.");
                continue;
            }

            ValidateNotFirstInTask(where, onPage, questions, problems);
            int pageSectionOrder = questions[onPage.Field.Id].SectionOrder;
            if (
                (
                    trigger.SectionOrder,
                    trigger.Question.Task.Order,
                    trigger.Question.Page.Order
                ).CompareTo((pageSectionOrder, onPage.Task.Order, onPage.Page.Order)) >= 0
            )
            {
                problems.Add(
                    $"{where} names '{condition.QuestionId}', which must be on an earlier page."
                );
            }

            ValidateTrigger(where, trigger.Question, condition.Values, problems);
        }
    }

    /// <summary>The task list links to a task's first page, so it must always apply.</summary>
    private static void ValidateNotFirstInTask(
        string where,
        QuestionDeclaration onPage,
        Dictionary<string, (QuestionDeclaration Question, int SectionOrder)> questions,
        List<string> problems
    )
    {
        int firstPageOrder = questions
            .Values.Where(x =>
                string.Equals(x.Question.Task.Id, onPage.Task.Id, StringComparison.Ordinal)
            )
            .Min(static x => x.Question.Page.Order);
        if (onPage.Page.Order == firstPageOrder)
        {
            problems.Add(
                $"{where} is on the first page of task '{onPage.Task.Id}', which must always apply."
            );
        }
    }

    /// <summary>
    /// A trigger is a choice on an enum (values are its members), a choice on reference data with
    /// a <c>string Code</c> (values are codes), or a date (values are <see cref="DatePrecision"/>
    /// members).
    /// </summary>
    private static void ValidateTrigger(
        string where,
        QuestionDeclaration trigger,
        IReadOnlyList<object> values,
        List<string> problems
    )
    {
        Type? valueType = TriggerValueType(trigger);
        if (valueType is null)
        {
            problems.Add(
                $"{where} names '{trigger.Field.Id}', which must be a Radio, Select or Checkbox on an enum or on reference data with a string Code, or a PrecisionDate."
            );
            return;
        }

        if (values.Count == 0)
        {
            problems.Add($"{where} needs at least one value.");
        }

        foreach (object value in values)
        {
            if (valueType == typeof(string))
            {
                if (value is not string code || string.IsNullOrWhiteSpace(code))
                {
                    problems.Add(
                        $"{where} has value '{value}', which is not a {trigger.Binding.ReferenceType!.Name} code."
                    );
                }
            }
            else if (value.GetType() != valueType || !Enum.IsDefined(valueType, value))
            {
                problems.Add(
                    $"{where} has value '{value}', which is not a {valueType.Name} member."
                );
            }
        }

        if (values.Distinct().Count() != values.Count)
        {
            problems.Add($"{where} repeats a value.");
        }
    }

    /// <summary>The type of the values a condition on the trigger names, or null if it cannot be a trigger.</summary>
    private static Type? TriggerValueType(QuestionDeclaration trigger)
    {
        if (trigger.Control == FormControl.PrecisionDate)
        {
            return typeof(DatePrecision);
        }

        if (
            trigger.Control is not (FormControl.Radio or FormControl.Select or FormControl.Checkbox)
        )
        {
            return null;
        }

        if (trigger.Binding.EnumType is { } enumType)
        {
            return enumType;
        }

        return trigger.Binding.ReferenceType?.GetProperty("Code")?.PropertyType == typeof(string)
            ? typeof(string)
            : null;
    }

    private static void ValidatePageMetadata(
        string form,
        IGrouping<
            (string SectionId, string TaskId, string Id),
            (QuestionDeclaration Question, string SectionId)
        >[] pages,
        List<string> problems
    )
    {
        foreach (var page in pages)
        {
            if (
                page.Select(static x =>
                        (
                            x.Question.Page.Title,
                            x.Question.Page.Order,
                            x.Question.Page.CommercialInConfidence
                        )
                    )
                    .Distinct()
                    .Count() > 1
            )
            {
                problems.Add(
                    $"{form} declares page '{page.Key.Id}' with different titles, orders or commercial in confidence flags."
                );
            }
        }
    }

    private static void ReportTies<T>(
        IEnumerable<T> items,
        Func<T, int> order,
        Func<T, string> describe,
        string where,
        List<string> problems
    )
    {
        foreach (IGrouping<int, T> tie in items.GroupBy(order).Where(static g => g.Count() > 1))
        {
            problems.Add(
                $"{where} has {string.Join(" and ", tie.Select(describe))} with the same Order {tie.Key}."
            );
        }
    }
}
