using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.CrossPageRules;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;

namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>
/// Checks a whole revision before it is submitted: every page rule on every page that applies, so
/// unanswered required questions are caught (except in an optional section, where nothing is
/// required), then every cross-page rule. One error per question, in form order.
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
            CheckPage(model, page, answers, options, applicability, today, errors);
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
    /// Checks one page. Nothing in an optional section is required, even once the user has started
    /// it, so a question there with no answer raises no error; an answer that was given is still
    /// checked. An empty answer can only fail Required, as every other rule passes on it.
    /// </summary>
    private static void CheckPage(
        FormModel model,
        FormPage page,
        IReadOnlyDictionary<string, FormAnswer> answers,
        IReadOnlyDictionary<string, IReadOnlyList<FormOption>> options,
        Applicability applicability,
        DateOnly today,
        Dictionary<string, string> errors
    )
    {
        if (!IsInOptionalSection(model, page))
        {
            PageAnswerValidator.Check(page, answers, options, applicability, today, errors);
            return;
        }

        Dictionary<string, string> found = new(StringComparer.Ordinal);
        PageAnswerValidator.Check(page, answers, options, applicability, today, found);
        foreach (var (key, message) in found)
        {
            // Keys of list entries and group items (for example "key[0]") are not answers of their
            // own, so they are missing from answers; they come from an answer that was given.
            if (!answers.TryGetValue(key, out FormAnswer? answer) || !answer.IsEmpty)
            {
                errors[key] = message;
            }
        }
    }

    // TODO: service readiness becomes mandatory within 12 months of the market authorisation date.
    // Until that date exists on the record, "optional" is a fixed property of the section; the rule
    // needs it to depend on the record. The task list and check your answers pages read it from the
    // response (`optional` on the section).
    private static bool IsInOptionalSection(FormModel model, FormPage page) =>
        model.Sections.Any(s =>
            s.Optional && string.Equals(s.Id, page.SectionId, StringComparison.Ordinal)
        );
}
