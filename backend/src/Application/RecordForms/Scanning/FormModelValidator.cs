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

        ValidateOptions(question, control, options, problems);
        ValidateRules(question, control, problems);
        ValidatePresentation(question, control, problems);
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

    private static void ValidatePresentation(
        QuestionDeclaration question,
        FormControl control,
        List<string> problems
    )
    {
        string where = question.Where;
        FormFieldAttribute field = question.Field;
        bool isHierarchical = control == FormControl.HierarchicalSelect;

        if (
            field.Placeholder is not null
            && control is not (FormControl.Select or FormControl.HierarchicalSelect)
        )
        {
            problems.Add(
                $"{where} sets Placeholder, which only a Select or HierarchicalSelect uses."
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

        foreach (
            var duplicate in questions
                .GroupBy(static x => x.Question.Field.Id, StringComparer.Ordinal)
                .Where(static g => g.Count() > 1)
        )
        {
            problems.Add($"{form} uses question ID '{duplicate.Key}' more than once.");
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
        foreach (var page in pages)
        {
            if (
                page.Select(static x => (x.Question.Page.Title, x.Question.Page.Order))
                    .Distinct()
                    .Count() > 1
            )
            {
                problems.Add(
                    $"{form} declares page '{page.Key.Id}' with different titles or orders."
                );
            }

            ReportTies(
                page,
                static x => x.Question.Field.Order,
                static x => $"question '{x.Question.Field.Id}'",
                $"{form}, page '{page.Key.Id}',",
                problems
            );
        }

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
