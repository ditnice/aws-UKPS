using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.CrossPageRules;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;

namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>
/// Checks a whole revision before it is submitted: every page rule on every page that applies, so
/// unanswered required questions are caught, then every cross-page rule. One error per question,
/// in form order.
/// </summary>
internal static class RevisionValidator
{
    public static IReadOnlyList<SubmissionErrorDto> Validate(
        FormModel model,
        IReadOnlyDictionary<string, FormAnswer> answers,
        Applicability applicability,
        IReadOnlyDictionary<string, IReadOnlyList<FormOption>> options,
        IEnumerable<CrossPageRule> rules,
        DateOnly today
    )
    {
        Dictionary<string, string> errors = new(StringComparer.Ordinal);
        foreach (FormPage page in model.Pages.Where(applicability.Applies))
        {
            if (IsUntouchedOptionalTask(model, page, answers, applicability))
            {
                continue;
            }
            PageAnswerValidator.Check(page, answers, options, applicability, today, errors);
        }

        // A field with a page rule error keeps it, so it shows one message, and its value isn't
        // trusted for a comparison.
        HashSet<string> pageErrors = [.. errors.Keys];
        foreach (CrossPageRule rule in rules)
        {
            if (rule.QuestionIds.Any(pageErrors.Contains))
            {
                continue;
            }

            foreach (var (questionId, message) in rule.Check(model, answers, applicability))
            {
                errors.TryAdd(questionId, message);
            }
        }

        return model
            .Questions.Where(q => errors.ContainsKey(q.Id))
            .Select(q =>
            {
                FormPage page = model.PageOf(q);
                return new SubmissionErrorDto
                {
                    QuestionId = q.Id,
                    PageId = page.Id,
                    TaskId = page.TaskId,
                    Message = errors[q.Id],
                };
            })
            .ToArray();
    }

    /// <summary>
    /// A task in an optional section with nothing answered raises no errors; once any question in it
    /// is answered, its required rules apply as usual.
    /// </summary>
    private static bool IsUntouchedOptionalTask(
        FormModel model,
        FormPage page,
        IReadOnlyDictionary<string, FormAnswer> answers,
        Applicability applicability
    )
    {
        FormSection section = model.Sections.First(s =>
            s.Tasks.Any(t => string.Equals(t.Id, page.TaskId, StringComparison.Ordinal))
        );
        if (!section.Optional)
        {
            return false;
        }

        FormTask task = section.Tasks.First(t =>
            string.Equals(t.Id, page.TaskId, StringComparison.Ordinal)
        );
        return task.Questions.Where(applicability.Applies).All(q => answers[q.Id].IsEmpty);
    }
}
