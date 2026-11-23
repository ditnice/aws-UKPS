using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms.Conditions;

/// <summary>
/// Met when the trigger question's answer includes none of <see cref="Values"/>: the single
/// answer, or every ticked value of a multi-value answer. An unanswered trigger, or one hidden by
/// its own condition, includes none of them, so it is met.
/// </summary>
internal sealed record AnswerNotInCondition(string QuestionId, IReadOnlyList<string> Values)
    : FormCondition
{
    public override ConditionKind Kind => ConditionKind.AnswerNotIn;

    public override IReadOnlyList<string> TriggerQuestionIds => [QuestionId];

    public override string Description => $"{Kind} {QuestionId} [{string.Join(", ", Values)}]";

    public override bool IsMet(Func<string, FormAnswer> answerOf) =>
        !answerOf(QuestionId)
            .AllValues()
            .Any(value => Values.Contains(value, StringComparer.Ordinal));
}
