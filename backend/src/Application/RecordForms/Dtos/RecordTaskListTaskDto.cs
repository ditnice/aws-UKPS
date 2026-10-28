namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A task and its current status.</summary>
public sealed record RecordTaskListTaskDto
{
    /// <summary>The task ID.</summary>
    public required string Id { get; init; }

    /// <summary>The task title.</summary>
    public required string Title { get; init; }

    /// <summary>
    /// The persisted-answer status: <c>not-started</c>, <c>in-progress</c> or <c>completed</c>.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>The ID of this task's first page.</summary>
    public required string FirstPageId { get; init; }
}
