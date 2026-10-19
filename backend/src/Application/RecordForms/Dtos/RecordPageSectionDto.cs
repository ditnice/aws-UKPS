namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>The section a page belongs to. Its title is the page caption.</summary>
public sealed record RecordPageSectionDto
{
    /// <summary>The section ID.</summary>
    public required string Id { get; init; }

    /// <summary>The section title, shown as the page caption.</summary>
    public required string Title { get; init; }
}
