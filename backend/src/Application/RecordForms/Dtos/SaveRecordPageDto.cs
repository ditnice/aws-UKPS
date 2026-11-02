namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>Where to go after saving a page.</summary>
public sealed record SaveRecordPageDto
{
    /// <summary>The next page, or null after the last page.</summary>
    public required string? NextPageId { get; init; }
}
