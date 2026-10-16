namespace UKPS.Api.Application.Forms.Dtos;

/// <summary>A page of a record content form.</summary>
public sealed record FormPageDto
{
    /// <summary>The page's identifier, used in its URL.</summary>
    public required string Id { get; init; }

    /// <summary>A short page title, for the browser title.</summary>
    public required string Title { get; init; }
}
