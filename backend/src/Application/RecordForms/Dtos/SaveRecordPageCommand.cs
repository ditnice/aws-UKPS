using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>Every answer on a page, with the versions the page was loaded at.</summary>
public sealed record SaveRecordPageCommand
{
    /// <summary>The <see cref="RecordPageDto.FormVersion"/> the page was loaded with.</summary>
    [Required]
    public required string FormVersion { get; init; }

    /// <summary>The <see cref="RecordPageDto.RevisionVersion"/> the page was loaded with.</summary>
    [Required]
    public required uint RevisionVersion { get; init; }

    /// <summary>
    /// Every question on the page, keyed by question ID: null for an unanswered single-value
    /// question and an empty array for an unanswered multi-value question.
    /// </summary>
    [Required]
    public required IReadOnlyDictionary<string, JsonElement> Answers { get; init; }
}
