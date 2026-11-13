using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Scanning;

namespace UKPS.Api.Application.RecordForms.CrossPageRules;

/// <summary>
/// One date must come before another, or on the same day when <see cref="AllowSame"/> is set.
/// Estimated dates are compared as the month or quarter they cover, so the rule only fails when
/// the two periods can't be in order: an earlier date in Q3 against a later date of 15 August
/// passes.
/// </summary>
/// <remarks>
/// The rule is skipped when <see cref="ExemptWhen"/> is met, or when either date doesn't apply,
/// is unanswered or isn't a real date, which the page rules report.
/// </remarks>
internal sealed record DateOrderRule(
    string Id,
    string EarlierQuestionId,
    string LaterQuestionId,
    bool AllowSame,
    string EarlierMessage,
    string LaterMessage,
    FormCondition? ExemptWhen = null
) : CrossPageRule(Id)
{
    public override IReadOnlyList<string> QuestionIds => [EarlierQuestionId, LaterQuestionId];

    public override IEnumerable<(string QuestionId, string Message)> Check(
        FormModel model,
        IReadOnlyDictionary<string, FormAnswer> answers,
        Applicability applicability
    )
    {
        if (ExemptWhen is not null && applicability.IsMet(ExemptWhen))
        {
            return [];
        }

        if (
            !TryGetRange(
                model.FindQuestion(EarlierQuestionId)!,
                answers,
                applicability,
                out DateOnly earlierFirst,
                out _
            )
            || !TryGetRange(
                model.FindQuestion(LaterQuestionId)!,
                answers,
                applicability,
                out _,
                out DateOnly laterLast
            )
        )
        {
            return [];
        }

        bool inOrder = AllowSame ? earlierFirst <= laterLast : earlierFirst < laterLast;
        return inOrder
            ? []
            : [(EarlierQuestionId, EarlierMessage), (LaterQuestionId, LaterMessage)];
    }

    public override IEnumerable<string> Problems(FormModel model) =>
        base.Problems(model)
            .Concat(
                QuestionIds
                    .Select(model.FindQuestion)
                    .OfType<FormQuestion>()
                    .Where(static q => !ControlRules.IsDate(q.Control))
                    .Select(q => $"Cross-page rule '{Id}' compares '{q.Id}', which is not a date.")
            );

    private static bool TryGetRange(
        FormQuestion question,
        IReadOnlyDictionary<string, FormAnswer> answers,
        Applicability applicability,
        out DateOnly firstDay,
        out DateOnly lastDay
    )
    {
        firstDay = lastDay = default;
        return applicability.Applies(question)
            && answers.TryGetValue(question.Id, out FormAnswer? answer)
            && answer.Value is not null
            && DateAnswer.TryGetRange(question.Control, answer.Value, out firstDay, out lastDay);
    }
}
