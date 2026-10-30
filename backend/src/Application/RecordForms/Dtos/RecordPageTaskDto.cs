namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>The task a page belongs to. Its title is the page caption.</summary>
public sealed record RecordPageTaskDto
{
    /// <summary>The task ID.</summary>
    public required string Id { get; init; }

    /// <summary>The task title, shown as the page caption.</summary>
    public required string Title { get; init; }
}
