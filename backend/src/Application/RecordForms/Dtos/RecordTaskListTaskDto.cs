namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A task and its current status.</summary>
public sealed record RecordTaskListTaskDto
{
    /// <summary>The task ID.</summary>
    public required string Id { get; init; }

    /// <summary>The task title.</summary>
    public required string Title { get; init; }

    /// <summary>
    /// Hint text shown under the task link, or null for none. A blank line starts a new paragraph.
    /// </summary>
    public required string? Hint { get; init; }

    /// <summary>The task's status, derived from the saved answers.</summary>
    public required RecordTaskStatus Status { get; init; }

    /// <summary>The ID of this task's first page.</summary>
    public required string FirstPageId { get; init; }
}
