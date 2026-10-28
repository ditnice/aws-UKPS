namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A section of a record task list.</summary>
public sealed record RecordTaskListSectionDto
{
    /// <summary>The section ID.</summary>
    public required string Id { get; init; }

    /// <summary>The section title.</summary>
    public required string Title { get; init; }

    /// <summary>The ordered tasks in the section.</summary>
    public required IReadOnlyList<RecordTaskListTaskDto> Tasks { get; init; }
}
