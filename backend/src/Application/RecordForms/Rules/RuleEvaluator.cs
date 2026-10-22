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
            RuleKind.MaxLength => answer.Value is null || answer.Value.Length <= rule.Value,
            RuleKind.MaxItems => answer.Values.Count <= rule.Value,
            _ => throw new InvalidOperationException($"Unsupported rule kind {rule.Kind}."),
        };
    }
}
