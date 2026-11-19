namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>One item of a group question: a row for each of its child questions.</summary>
public sealed record RecordCheckAnswersGroupItemDto
{
    /// <summary>A row for each child question of the group, in order.</summary>
    public required IReadOnlyList<RecordCheckAnswersQuestionDto> Questions { get; init; }
}
