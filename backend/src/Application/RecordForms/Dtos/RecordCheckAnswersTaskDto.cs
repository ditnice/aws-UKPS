namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A task on check your answers.</summary>
public sealed record RecordCheckAnswersTaskDto
{
    /// <summary>The task ID.</summary>
    public required string Id { get; init; }

    /// <summary>The task title.</summary>
    public required string Title { get; init; }

    /// <summary>
    /// The questions that apply, in page order, including optional questions with no answer.
    /// </summary>
    public required IReadOnlyList<RecordCheckAnswersQuestionDto> Questions { get; init; }
}
