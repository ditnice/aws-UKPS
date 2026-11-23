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
/// <remarks>
/// Errors inside a text list or group are keyed by 0-based position, matching the frontend's
/// array fields: <c>question[1]</c> for a text list entry,
/// <c>group[0].child</c> for a group item's question and
/// <c>group[0].child[1]</c> for an entry of a text list in a group item.
/// </remarks>
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

            var (answer, error) = AnswerParser.Parse(question, element);
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
    /// adding one error per failing key to <paramref name="errors"/>. Questions that do not
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
                ?? CheckAnswer(question, question.Id, answer, errors);
            if (error is not null)
            {
                errors[question.Id] = error;
            }
        }
    }

    /// <summary>
    /// Checks the question's rules and, for a text list or group, its entries or items. Returns the
    /// error for the question itself; errors for entries and items are added under their own keys.
    /// </summary>
    private static string? CheckAnswer(
        FormQuestion question,
        string key,
        FormAnswer answer,
        Dictionary<string, string> errors
    )
    {
        switch (answer.Shape)
        {
            case AnswerShape.List:
                foreach (int repeat in RepeatIndexes(question, answer.Values))
                {
                    errors[$"{key}[{repeat}]"] = DistinctMessage(question)!;
                }

                return FirstFailure(question, answer);
            case AnswerShape.Group:
                CheckItems(question, key, answer, errors);
                return FirstFailure(question, answer);
            default:
                return RuleEvaluator.FirstFailure(question.Rules, answer)
                    ?? CheckPrefixedNumber(question, answer);
        }
    }

    /// <summary>
    /// Checks each item of a group: every child's own rules, then that it does not repeat an
    /// earlier item's answer where it must be distinct.
    /// </summary>
    private static void CheckItems(
        FormQuestion group,
        string key,
        FormAnswer answer,
        Dictionary<string, string> errors
    )
    {
        foreach (FormQuestion child in group.Children)
        {
            HashSet<int> repeats =
            [
                .. RepeatIndexes(child, answer.Items.Select(i => i[child.Id].Value)),
            ];
            for (int i = 0; i < answer.Items.Count; i++)
            {
                string childKey = $"{key}[{i}].{child.Id}";
                string? error =
                    CheckAnswer(child, childKey, answer.Items[i][child.Id], errors)
                    ?? (repeats.Contains(i) ? DistinctMessage(child) : null);
                if (error is not null)
                {
                    errors[childKey] = error;
                }
            }
        }
    }

    /// <summary>The first failing rule other than Distinct, which is reported per repeat.</summary>
    private static string? FirstFailure(FormQuestion question, FormAnswer answer) =>
        RuleEvaluator.FirstFailure(
            question.Rules.Where(static r => r.Kind != RuleKind.Distinct),
            answer
        );

    private static IEnumerable<int> RepeatIndexes(
        FormQuestion question,
        IEnumerable<string?> values
    ) => DistinctMessage(question) is null ? [] : RuleEvaluator.RepeatIndexes(values);

    private static string? DistinctMessage(FormQuestion question) =>
        question.Rules.FirstOrDefault(static r => r.Kind == RuleKind.Distinct)?.Message;

    /// <summary>
    /// A date answer must be a real date: an actual date or month not after today, an estimate not
    /// yet past.
    /// </summary>
    private static string? CheckDate(FormQuestion question, FormAnswer answer, DateOnly today) =>
        ControlRules.IsDate(question.Control) && answer.Value is not null
            ? DateAnswer.Check(question.Control, answer.Value, today, question.DateName)
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
    /// stored, including inside group items. Its format error is left to <see cref="Check"/>,
    /// which reports it only when the question applies.
    /// </summary>
    private static FormAnswer Normalise(FormQuestion question, FormAnswer answer)
    {
        if (answer.Shape == AnswerShape.Group)
        {
            return FormAnswer.Group(
                answer.Items.Select(item => new FormGroupItem(
                    question.Children.ToDictionary(
                        static c => c.Id,
                        c => Normalise(c, item[c.Id]),
                        StringComparer.Ordinal
                    )
                ))
            );
        }

        return question.Prefixes is { } prefixes
            ? FormAnswer.Single(
                PrefixedNumberAnswer
                    .Evaluate(prefixes, question.PrefixDigits, question.NumberName!, answer.Value)
                    .Value
            )
            : answer;
    }

    private static string? CheckPrefixedNumber(FormQuestion question, FormAnswer answer) =>
        question.Prefixes is { } prefixes
            ? PrefixedNumberAnswer
                .Evaluate(prefixes, question.PrefixDigits, question.NumberName!, answer.Value)
                .Error
            : null;
}
