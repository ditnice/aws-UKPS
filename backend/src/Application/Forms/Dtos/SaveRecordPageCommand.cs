using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace UKPS.Api.Application.Forms.Dtos;

/// <summary>
/// Saves every answer on a page of a record's content form.
/// </summary>
public sealed record SaveRecordPageCommand
{
    /// <summary>The <c>formVersion</c> the page was loaded with.</summary>
    [Required]
    public required string FormVersion { get; init; }

    /// <summary>The <c>revisionVersion</c> the page was loaded with.</summary>
    [Required]
    public required uint RevisionVersion { get; init; }

    /// <summary>
    /// An answer for every question on the page, keyed by question ID. Unanswered optional
    /// questions are <c>null</c> (or an empty array for checkboxes).
    /// </summary>
    [Required]
    public required IReadOnlyDictionary<string, JsonNode?> Answers { get; init; }
}
