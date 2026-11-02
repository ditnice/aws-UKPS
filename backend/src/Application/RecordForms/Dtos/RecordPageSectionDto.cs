namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>The section a page belongs to.</summary>
public sealed record RecordPageSectionDto
{
    /// <summary>The section ID.</summary>
    public required string Id { get; init; }

    /// <summary>The section title.</summary>
    public required string Title { get; init; }
}
