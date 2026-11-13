using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Conditions;

/// <summary>
/// Which pages and questions of a form apply to a revision, given its answers. Each page or
/// question is worked out when first asked about, so only the answers it depends on are read.
/// </summary>
/// <remarks>
/// A page applies when it has no condition or its condition is met. A question applies when its
/// page applies and its own condition, if any, is met. A trigger that does not apply reads as
/// unanswered, so chains of conditions cascade. Triggers always come earlier, so the recursion
/// ends. A date trigger is compared by its precision, and a reference data trigger by its code,
/// using <paramref name="codes"/>.
/// </remarks>
internal sealed class Applicability(
    FormModel model,
    IReadOnlyDictionary<string, FormAnswer> answers,
    ReferenceCodes codes
)
{
    private readonly Dictionary<string, bool> _pages = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _questions = new(StringComparer.Ordinal);

    public bool Applies(FormPage page)
    {
        if (!_pages.TryGetValue(page.Id, out bool applies))
        {
            applies = page.ShowWhen is null || page.ShowWhen.IsMet(AnswerOf);
            _pages[page.Id] = applies;
        }

        return applies;
    }

    public bool Applies(FormQuestion question)
    {
        if (!_questions.TryGetValue(question.Id, out bool applies))
        {
            applies =
                Applies(model.PageOf(question)) && (question.ShowWhen?.IsMet(AnswerOf) != false);
            _questions[question.Id] = applies;
        }

        return applies;
    }

    /// <summary>
    /// Whether a condition holds for these answers, reading its triggers as page and question
    /// conditions do.
    /// </summary>
    public bool IsMet(FormCondition condition) => condition.IsMet(AnswerOf);

    /// <summary>
    /// What a condition compares: the trigger's answer (a date's precision, or a reference data
    /// item's code), or unanswered when the trigger does not apply.
    /// </summary>
    private FormAnswer AnswerOf(string questionId)
    {
        FormQuestion trigger = model.FindQuestion(questionId)!;
        if (!Applies(trigger))
        {
            return FormAnswer.Empty(trigger.Shape);
        }

        if (!answers.TryGetValue(questionId, out FormAnswer? answer))
        {
            throw new InvalidOperationException(
                $"The answer to trigger question '{questionId}' was not loaded."
            );
        }

        if (trigger.Control == FormControl.PrecisionDate)
        {
            return FormAnswer.Single(DateAnswer.PrecisionOf(answer.Value));
        }

        return trigger.ReferenceType is { } referenceType && !answer.IsEmpty
            ? CodesOf(referenceType, answer)
            : answer;
    }

    private FormAnswer CodesOf(Type referenceType, FormAnswer answer)
    {
        string[] values = [.. codes.CodesOf(referenceType, answer.AllValues())];
        return answer.Shape == AnswerShape.Single
            ? FormAnswer.Single(values.SingleOrDefault())
            : FormAnswer.Multi(values);
    }
}
