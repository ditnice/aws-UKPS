namespace UKPS.Api.Application.Forms.Dtos;

/// <summary>The result of saving a page of a record's content form.</summary>
public sealed record SaveRecordPageDto
{
    /// <summary>
    /// The page to show next, or <c>null</c> when the saved page was the last one.
    /// </summary>
    public required string? NextPageId { get; init; }
}
