using System.Text.Json;

namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>One page of a record content form: its questions, options and current answers.</summary>
public sealed record RecordPageDto
{
    /// <summary>The form's version. Echo it back on save.</summary>
    public required string FormVersion { get; init; }

    /// <summary>
    /// The concurrency token of the record's latest revision, which the page reads and writes.
    /// Echo it back on save.
    /// </summary>
    public required uint RevisionVersion { get; init; }

    /// <summary>The organisation that owns the record; used for links back to its records list.</summary>
    public required int OrganisationId { get; init; }

    /// <summary>The record the page belongs to.</summary>
    public required int RecordId { get; init; }

    /// <summary>The section the page belongs to.</summary>
    public required RecordPageSectionDto Section { get; init; }

    /// <summary>The page's ID and title.</summary>
    public required RecordPagePageDto Page { get; init; }

    /// <summary>The target of Back, or null on the first page.</summary>
    public required string? PreviousPageId { get; init; }

    /// <summary>The questions on the page, in order.</summary>
    public required IReadOnlyList<RecordPageQuestionDto> Questions { get; init; }

    /// <summary>
    /// Every question on the page, keyed by question ID: a string or null, or an array of strings
    /// for multi-value questions.
    /// </summary>
    public required IReadOnlyDictionary<string, JsonElement> Answers { get; init; }
}
