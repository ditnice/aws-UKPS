using System.Text.Json;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Scanning;

namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>
/// Server-side validation of a submitted page in two steps: <see cref="Parse"/> checks every
/// question is present with the right shape and there are no other keys, and normalises
/// <c>PrefixedNumber</c> answers; <see cref="Check"/> then checks, for each question that applies,
/// a date is a real date on the right side of today, every value is an offered option, the
/// question's rules pass and a <c>PrefixedNumber</c> is well formed. One message per key.
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
                answers[question.Id] = Normalise(question, answer!);
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
        DateOnly today,
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
                CheckDate(question, answer, today)
                ?? CheckOptions(answer, options.GetValueOrDefault(question.Id))
                ?? RuleEvaluator.FirstFailure(question.Rules, answer)
                ?? CheckPrefixedNumber(question, answer);
            if (error is not null)
            {
                errors[question.Id] = error;
            }
        }
    }

    /// <summary>
    /// A date answer must be a real date: an actual date or month not after today, an estimate not
    /// yet past.
    /// </summary>
    private static string? CheckDate(FormQuestion question, FormAnswer answer, DateOnly today) =>
        ControlRules.IsDate(question.Control) && answer.Value is not null
            ? DateAnswer.Check(question.Control, answer.Value, today)
            : null;

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

    /// <summary>
    /// Saves a <c>PrefixedNumber</c> prefix with no digits as blank, so the rules see what will be
    /// stored. Its format error is left to <see cref="Check"/>, which reports it only when the
    /// question applies.
    /// </summary>
    private static FormAnswer Normalise(FormQuestion question, FormAnswer answer) =>
        question.Prefixes is { } prefixes
            ? FormAnswer.Single(PrefixedNumberAnswer.Evaluate(prefixes, answer.Value).Value)
            : answer;

    private static string? CheckPrefixedNumber(FormQuestion question, FormAnswer answer) =>
        question.Prefixes is { } prefixes
            ? PrefixedNumberAnswer.Evaluate(prefixes, answer.Value).Error
            : null;
}
