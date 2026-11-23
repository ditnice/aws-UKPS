using UKPS.Api.Application.RecordForms.Conditions;

namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A condition that decides whether a question applies, from another question's answer.</summary>
public sealed record RecordPageConditionDto
{
    /// <summary>How the condition is evaluated.</summary>
    public required ConditionKind Kind { get; init; }

    /// <summary>The ID of the trigger question, an earlier question on the same page.</summary>
    public required string QuestionId { get; init; }

    /// <summary>
    /// For <see cref="ConditionKind.AnswerIn"/>: the condition is met when the trigger's answer
    /// includes any of these option values.
    /// </summary>
    public required IReadOnlyList<string> Values { get; init; }
}
