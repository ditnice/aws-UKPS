namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A page's ID, title and "Commercial in confidence" flag.</summary>
public sealed record RecordPagePageDto
{
    /// <summary>The page ID, used in the URL.</summary>
    public required string Id { get; init; }

    /// <summary>A short name for the page.</summary>
    public required string Title { get; init; }

    /// <summary>Whether the page shows the "Commercial in confidence" badge.</summary>
    public required bool CommercialInConfidence { get; init; }
}
