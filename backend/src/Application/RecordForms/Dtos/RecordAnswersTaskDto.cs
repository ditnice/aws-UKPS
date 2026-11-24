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
    /// Empty when <see cref="NotApplicable"/>.
    /// </summary>
    public required IReadOnlyList<RecordAnswersQuestionDto> Questions { get; init; }

    /// <summary>
    /// No question in the task applies to this record, for example Promising Innovative Medicine
    /// (PIM) and Early Access to Medicines Scheme (EAMS) on a biosimilar. Check your answers says
    /// why; the task list shows "Cannot start yet".
    /// </summary>
    public required bool NotApplicable { get; init; }
}
