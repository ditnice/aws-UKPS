using System.Text.Json.Nodes;
using UKPS.Api.Application.Forms.Definitions;

namespace UKPS.Api.Application.Forms;

/// <summary>
/// Server-side checks on a submitted page. Errors are keyed by question ID, matching the keys
/// clients use for fields.
/// </summary>
internal static class PageAnswerValidator
{
    public const string MissingAnswer =
        "Provide an answer for this question, or null if it is unanswered.";
    public const string UnknownQuestion = "This is not a question on this page.";
    public const string WrongFormat = "The answer is not in the expected format.";
    public const string InvalidOption = "Select a valid option.";

    /// <summary>
    /// Checks that every page question (and nothing else) has an answer of the right JSON shape.
    /// </summary>
    public static Dictionary<string, string[]> CheckShape(
        PageDefinition page,
        IReadOnlyDictionary<string, JsonNode?> answers
    )
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var questionIds = page.Questions.Select(q => q.Id).ToHashSet(StringComparer.Ordinal);

        foreach (var key in answers.Keys.Where(key => !questionIds.Contains(key)))
        {
            errors[key] = [UnknownQuestion];
        }

        foreach (var question in page.Questions)
        {
            if (!answers.TryGetValue(question.Id, out var answer))
            {
                errors[question.Id] = [MissingAnswer];
            }
            else if (!question.Type.IsValidShape(answer))
            {
                errors[question.Id] = [WrongFormat];
            }
        }

        return errors;
    }

    /// <summary>
    /// Runs each question's rules against its normalised answer and adds the first failure for
    /// each question that does not already have an error.
    /// </summary>
    public static void ValidateRules(
        PageDefinition page,
        IReadOnlyDictionary<string, JsonNode?> answers,
        Dictionary<string, string[]> errors
    )
    {
        foreach (var question in page.Questions.Where(q => !errors.ContainsKey(q.Id)))
        {
            var failed = question.Rules.FirstOrDefault(rule =>
                !rule.IsSatisfiedBy(answers[question.Id])
            );
            if (failed is not null)
            {
                errors[question.Id] = [failed.Message];
            }
        }
    }
}
