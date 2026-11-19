namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>One item of a group answer, such as a clinical trial.</summary>
public sealed record RecordAnswersGroupItemDto
{
    /// <summary>The answer to each of the group's questions, in order.</summary>
    public required IReadOnlyList<RecordAnswersQuestionDto> Questions { get; init; }
}
