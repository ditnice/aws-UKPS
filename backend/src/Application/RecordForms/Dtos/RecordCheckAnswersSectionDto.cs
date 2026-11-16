namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A section on check your answers.</summary>
public sealed record RecordCheckAnswersSectionDto
{
    /// <summary>The section ID.</summary>
    public required string Id { get; init; }

    /// <summary>The section title.</summary>
    public required string Title { get; init; }

    /// <summary>The tasks with a question that applies, in order.</summary>
    public required IReadOnlyList<RecordCheckAnswersTaskDto> Tasks { get; init; }
}
