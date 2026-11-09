using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms.Conditions;

/// <summary>
/// Met when the trigger question's answer includes any of <see cref="Values"/>: the single
/// answer, or any ticked value of a multi-value answer.
/// </summary>
internal sealed record AnswerInCondition(string QuestionId, IReadOnlyList<string> Values)
    : FormCondition
{
    public override ConditionKind Kind => ConditionKind.AnswerIn;

    public override IReadOnlyList<string> TriggerQuestionIds => [QuestionId];

    public override string Description => $"{Kind} {QuestionId} [{string.Join(", ", Values)}]";

    public override bool IsMet(Func<string, FormAnswer> answerOf) =>
        answerOf(QuestionId)
            .AllValues()
            .Any(value => Values.Contains(value, StringComparer.Ordinal));
}
