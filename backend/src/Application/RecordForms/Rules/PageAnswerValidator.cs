using System.Text.Json;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;

namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>
/// Server-side validation of a submitted page in two steps: <see cref="Parse"/> checks every
/// question is present with the right shape and there are no other keys; <see cref="Check"/>
/// then checks, for each question that applies, every value is an offered option and the
/// question's rules pass. One message per key.
/// </summary>
internal static class PageAnswerValidator
{
    /// <summary>
    /// Parses the submitted page into the normalised answers of the questions that parsed, with
    /// one error per failing key.
    /// </summary>
    public static (Dictionary<string, FormAnswer> Answers, Dictionary<string, string> Errors) Parse(
        FormPage page,
        IReadOnlyDictionary<string, JsonElement> submitted
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

    /// <summary>
    /// Checks each parsed answer of a question that applies against its options and rules,
    /// adding one error per failing question to <paramref name="errors"/>. Questions that do not
    /// apply are not checked.
    /// </summary>
    public static void Check(
        FormPage page,
        IReadOnlyDictionary<string, FormAnswer> answers,
        IReadOnlyDictionary<string, IReadOnlyList<FormOption>> options,
        Applicability applicability,
        Dictionary<string, string> errors
    )
    {
        foreach (FormQuestion question in page.Questions.Where(applicability.Applies))
        {
            if (!answers.TryGetValue(question.Id, out FormAnswer? answer))
            {
                continue;
            }

            string? error =
                CheckOptions(answer, options.GetValueOrDefault(question.Id))
                ?? RuleEvaluator.FirstFailure(question.Rules, answer);
            if (error is not null)
            {
                errors[question.Id] = error;
            }
        }
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
