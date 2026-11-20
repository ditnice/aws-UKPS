using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms.Conditions;

/// <summary>
/// Met when the trigger question is answered and its answer includes none of
/// <see cref="Values"/>: the single answer, or every ticked value of a multi-value answer.
/// </summary>
internal sealed record AnswerNotInCondition(string QuestionId, IReadOnlyList<string> Values)
    : FormCondition
{
    public override ConditionKind Kind => ConditionKind.AnswerNotIn;

    public override IReadOnlyList<string> TriggerQuestionIds => [QuestionId];

    public override string Description => $"{Kind} {QuestionId} [{string.Join(", ", Values)}]";

    public override bool IsMet(Func<string, FormAnswer> answerOf)
    {
        string[] chosen = [.. answerOf(QuestionId).AllValues()];
        return chosen.Length > 0
            && !chosen.Any(value => Values.Contains(value, StringComparer.Ordinal));
    }
}
