using System.Text.Json;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;

namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>
/// Server-side validation of a submitted page: every question present with the right shape, no
/// other keys, every value an offered option, then each question's rules. One message per key.
/// </summary>
internal static class PageAnswerValidator
{
    public static (
        Dictionary<string, FormAnswer> Answers,
        Dictionary<string, string> Errors
    ) Validate(
        FormPage page,
        IReadOnlyDictionary<string, JsonElement> submitted,
        IReadOnlyDictionary<string, IReadOnlyList<FormOption>> options
    )
    {
        Dictionary<string, FormAnswer> answers = new(StringComparer.Ordinal);
        Dictionary<string, string> errors = new(StringComparer.Ordinal);

        foreach (string key in submitted.Keys)
        {
            if (!page.Questions.Any(q => string.Equals(q.Id, key, StringComparison.Ordinal)))
            {
                errors[key] = RecordFormMessages.NotOnPage;
            }
        }

        foreach (FormQuestion question in page.Questions)
        {
            if (!submitted.TryGetValue(question.Id, out JsonElement element))
            {
                errors[question.Id] = RecordFormMessages.Missing;
                continue;
            }

            var (answer, error) = AnswerParser.Parse(question.Shape, element);
            error ??= CheckOptions(answer!, options.GetValueOrDefault(question.Id));
            error ??= RuleEvaluator.FirstFailure(question.Rules, answer!);

            if (error is not null)
            {
                errors[question.Id] = error;
            }
            else
            {
                answers[question.Id] = answer!;
            }
        }

        return (answers, errors);
    }

    private static string? CheckOptions(FormAnswer answer, IReadOnlyList<FormOption>? options)
    {
        if (options is null)
        {
            return null;
        }

        HashSet<string> offered = options
            .SelectMany(static o => o.SelfAndChildren())
            .Select(static o => o.Value)
            .ToHashSet(StringComparer.Ordinal);
        return answer.AllValues().All(offered.Contains) ? null : RecordFormMessages.InvalidOption;
    }
}
