namespace UKPS.Api.Application.Forms.Dtos;

/// <summary>A section of a record content form.</summary>
public sealed record FormSectionDto
{
    /// <summary>The section's stable identifier.</summary>
    public required string Id { get; init; }

    /// <summary>The section title, shown as the page caption.</summary>
    public required string Title { get; init; }
}
