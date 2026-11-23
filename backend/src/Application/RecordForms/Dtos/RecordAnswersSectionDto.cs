namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A section of a record's answers.</summary>
public sealed record RecordAnswersSectionDto
{
    /// <summary>The section ID.</summary>
    public required string Id { get; init; }

    /// <summary>The section title.</summary>
    public required string Title { get; init; }

    /// <summary>The tasks with a question that applies, in order.</summary>
    public required IReadOnlyList<RecordAnswersTaskDto> Tasks { get; init; }
}
