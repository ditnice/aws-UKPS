namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A task of a record's answers.</summary>
public sealed record RecordAnswersTaskDto
{
    /// <summary>The task ID.</summary>
    public required string Id { get; init; }

    /// <summary>The task title.</summary>
    public required string Title { get; init; }

    /// <summary>
    /// The questions that apply, in page order, including optional questions with no answer.
    /// </summary>
    public required IReadOnlyList<RecordAnswersQuestionDto> Questions { get; init; }
}
