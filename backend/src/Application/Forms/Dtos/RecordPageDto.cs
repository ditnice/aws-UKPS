using System.Text.Json.Nodes;

namespace UKPS.Api.Application.Forms.Dtos;

/// <summary>
/// One page of a record's content form: its structure, rules, options and current answers.
/// </summary>
public sealed record RecordPageDto
{
    /// <summary>
    /// The form definition version. Echo it when saving; a save from an older version is rejected.
    /// </summary>
    public required string FormVersion { get; init; }

    /// <summary>
    /// The revision's concurrency token. Echo it when saving; a save based on stale data is
    /// rejected.
    /// </summary>
    public required uint RevisionVersion { get; init; }

    /// <summary>
    /// Whether the page must be shown read-only: the revision is not a draft, or the caller
    /// cannot edit the record's content.
    /// </summary>
    public required bool ReadOnly { get; init; }

    /// <summary>The identifier of the organisation that owns the record.</summary>
    public required int OrganisationId { get; init; }

    /// <summary>The section the page belongs to.</summary>
    public required FormSectionDto Section { get; init; }

    /// <summary>The page itself.</summary>
    public required FormPageDto Page { get; init; }

    /// <summary>The ID of the previous page in the form, or <c>null</c> on the first page.</summary>
    public required string? PreviousPageId { get; init; }

    /// <summary>The page's questions, in display order.</summary>
    public required IReadOnlyList<FormQuestionDto> Questions { get; init; }

    /// <summary>
    /// The current answer for every question on the page, keyed by question ID: a string or
    /// <c>null</c> for single-value questions, an array of option values for checkboxes.
    /// </summary>
    public required IReadOnlyDictionary<string, JsonNode?> Answers { get; init; }

    /// <summary>
    /// Read-only answers from earlier pages that this page's conditions or rules refer to,
    /// keyed by question ID.
    /// </summary>
    public required IReadOnlyDictionary<string, JsonNode?> Context { get; init; }
}
