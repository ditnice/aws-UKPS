namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A section of a record task list.</summary>
public sealed record RecordTaskListSectionDto
{
    /// <summary>The section ID.</summary>
    public required string Id { get; init; }

    /// <summary>The section title.</summary>
    public required string Title { get; init; }

    /// <summary>
    /// Text shown under the section heading, or null for none. A blank line starts a new
    /// paragraph.
    /// </summary>
    public required string? SubText { get; init; }

    /// <summary>The ordered tasks in the section.</summary>
    public required IReadOnlyList<RecordTaskListTaskDto> Tasks { get; init; }
}
