namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>Where to go after saving a page.</summary>
public sealed record SaveRecordPageDto
{
    /// <summary>The next page in the task, or null after the task's last page.</summary>
    public required string? NextPageId { get; init; }
}
