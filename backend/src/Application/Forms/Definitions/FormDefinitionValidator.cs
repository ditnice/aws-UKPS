using System.Text.RegularExpressions;
using UKPS.Api.Application.Forms.Bindings;

namespace UKPS.Api.Application.Forms.Definitions;

/// <summary>
/// Structural checks run when definitions are registered, so a broken definition fails at
/// startup (and in unit tests) rather than on a user's save.
/// </summary>
internal static partial class FormDefinitionValidator
{
    private static readonly Dictionary<QuestionType, BindingValueKind[]> _allowedBindings = new()
    {
        [QuestionType.Textarea] = [BindingValueKind.Text],
        [QuestionType.Radio] = [BindingValueKind.Enum, BindingValueKind.ReferenceId],
        [QuestionType.Select] = [BindingValueKind.Enum, BindingValueKind.ReferenceId],
        [QuestionType.Checkbox] = [BindingValueKind.ReferenceIdList],
    };

    public static IReadOnlyList<string> Validate(FormDefinition form)
    {
        var errors = new List<string>();

        if (form.Pages.Count == 0)
        {
            errors.Add("The form has no pages.");
        }

        errors.AddRange(
            form.Sections.Where(section => section.Pages.Count == 0)
                .Select(section => $"Section '{section.Id}' has no pages.")
        );

        errors.AddRange(
            Duplicates(form.Pages.Select(page => page.Id))
                .Select(id => $"Duplicate page ID '{id}'.")
        );

        var questions = form.Pages.SelectMany(page => page.Questions).ToList();
        errors.AddRange(
            Duplicates(questions.Select(question => question.Id))
                .Select(id => $"Duplicate question ID '{id}'.")
        );

        foreach (var page in form.Pages)
        {
            if (!PageIdPattern().IsMatch(page.Id))
            {
                errors.Add($"Page ID '{page.Id}' must be a lowercase slug.");
            }

            if (page.Questions.Count == 0)
            {
                errors.Add($"Page '{page.Id}' has no questions.");
            }
        }

        foreach (var question in questions)
        {
            errors.AddRange(
                ValidateQuestion(question).Select(error => $"Question '{question.Id}': {error}")
            );
        }

        return errors;
    }

    /// <summary>Returns <paramref name="form"/> if it has no errors.</summary>
    /// <exception cref="InvalidOperationException">The definition has errors.</exception>
    public static FormDefinition EnsureValid(FormDefinition form)
    {
        var errors = Validate(form);
        return errors.Count == 0
            ? form
            : throw new InvalidOperationException(
                $"Form definition for {form.RecordType} is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}"
            );
    }

    private static IEnumerable<string> ValidateQuestion(QuestionDefinition question)
    {
        if (!QuestionIdPattern().IsMatch(question.Id))
        {
            yield return "ID must be snake_case 'table' or 'table.field'.";
        }

        var binding = question.Binding;
        if (!_allowedBindings[question.Type].Contains(binding.ValueKind))
        {
            yield return $"a {binding.ValueKind} binding cannot back a {question.Type} question.";
        }

        if (question.Type.HasOptions() != question.Options is not null)
        {
            yield return question.Type.HasOptions()
                ? $"{question.Type} questions need options."
                : $"{question.Type} questions cannot have options.";
        }

        if (question.Options is { } options)
        {
            if (binding.ValueKind == BindingValueKind.Enum && options.EnumType != binding.EnumType)
            {
                yield return $"options must be {binding.EnumType?.Name} enum options.";
            }

            if (binding.ValueKind != BindingValueKind.Enum && options.EnumType is not null)
            {
                yield return "enum options need an enum binding.";
            }
        }

        foreach (var rule in question.Rules.Where(rule => !rule.AppliesTo.Contains(question.Type)))
        {
            yield return $"rule '{rule.Kind}' does not apply to {question.Type} questions.";
        }

        foreach (var kind in Duplicates(question.Rules.Select(rule => rule.Kind)))
        {
            yield return $"rule '{kind}' is declared more than once.";
        }
    }

    private static IEnumerable<string> Duplicates(IEnumerable<string> values) =>
        values
            .GroupBy(value => value, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

    [GeneratedRegex(
        "^[a-z0-9]+(-[a-z0-9]+)*$",
        RegexOptions.ExplicitCapture,
        matchTimeoutMilliseconds: 1000
    )]
    private static partial Regex PageIdPattern();

    [GeneratedRegex(
        "^[a-z0-9_]+(\\.[a-z0-9_]+)?$",
        RegexOptions.ExplicitCapture,
        matchTimeoutMilliseconds: 1000
    )]
    private static partial Regex QuestionIdPattern();
}
