namespace UKPS.Api.Application.Forms.Definitions;

/// <summary>A titled group of pages.</summary>
internal sealed record SectionDefinition(
    string Id,
    string Title,
    IReadOnlyList<PageDefinition> Pages
);
