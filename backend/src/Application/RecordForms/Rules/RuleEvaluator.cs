using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>
/// Runs a question's rules in a fixed order by kind and reports only the first failure. The
/// frontend mirrors this in <c>lib/dynamic-form/answers.ts</c>; the shared fixture
/// <c>rule-cases.json</c> keeps the two in step.
/// </summary>
internal static class RuleEvaluator
{
    /// <summary>The message of the first failing rule, or null when every rule passes.</summary>
    public static string? FirstFailure(IEnumerable<FormRule> rules, FormAnswer answer)
    {
        return rules
            .OrderBy(static r => r.Kind)
            .FirstOrDefault(rule => !IsSatisfied(rule, answer))
            ?.Message;
    }

    public static bool IsSatisfied(FormRule rule, FormAnswer answer)
    {
        return rule.Kind switch
        {
            RuleKind.Required => !answer.IsEmpty,
            RuleKind.MaxLength => IsShortEnough(rule, answer.Value)
                && answer.Values.All(v => IsShortEnough(rule, v)),
            RuleKind.MaxItems => ItemCount(answer) <= rule.Value,
            RuleKind.Distinct => answer
                .Values.GroupBy(static v => v, StringComparer.OrdinalIgnoreCase)
                .All(static g => g.Count() == 1),
            _ => throw new InvalidOperationException($"Unsupported rule kind {rule.Kind}."),
        };
    }

    /// <summary>Whether a value, or one text list entry, is within a MaxLength rule.</summary>
    public static bool IsShortEnough(FormRule rule, string? value) =>
        value is null || value.Length <= rule.Value;

    private static int ItemCount(FormAnswer answer) =>
        answer.Shape == AnswerShape.Group ? answer.Items.Count : answer.Values.Count;

    /// <summary>
    /// The positions of values that repeat an earlier one, ignoring case, so the
    /// <see cref="RuleKind.Distinct"/> error can be shown against each repeat. Null values are
    /// never repeats.
    /// </summary>
    public static IEnumerable<int> RepeatIndexes(IEnumerable<string?> values)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        int index = 0;
        foreach (string? value in values)
        {
            if (value is not null && !seen.Add(value))
            {
                yield return index;
            }

            index++;
        }
    }
}
