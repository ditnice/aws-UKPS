namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A section of a record's answers.</summary>
public sealed record RecordAnswersSectionDto
{
    /// <summary>The section ID.</summary>
    public required string Id { get; init; }

    /// <summary>The section title.</summary>
    public required string Title { get; init; }

    /// <summary>
    /// The section is optional: its tasks are marked "(optional)", and no question in it is
    /// required, so an unanswered one raises no error and does not block submitting.
    /// </summary>
    public required bool Optional { get; init; }

    /// <summary>
    /// The section's tasks, in order. Check your answers also keeps tasks that do not apply;
    /// viewing a record leaves them out.
    /// </summary>
    public required IReadOnlyList<RecordAnswersTaskDto> Tasks { get; init; }
}
