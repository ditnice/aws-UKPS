namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A section on check your answers.</summary>
public sealed record RecordCheckAnswersSectionDto
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

    /// <summary>The section's tasks, in order, including tasks that do not apply.</summary>
    public required IReadOnlyList<RecordCheckAnswersTaskDto> Tasks { get; init; }
}
