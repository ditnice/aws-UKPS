using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A question that applies and its answer, as a row on check your answers.</summary>
public sealed record RecordCheckAnswersQuestionDto
{
    /// <summary>The question ID, which errors are keyed by.</summary>
    public required string Id { get; init; }

    /// <summary>The page the question is on, for its Change link.</summary>
    public required string PageId { get; init; }

    /// <summary>The question text.</summary>
    public required string Label { get; init; }

    /// <summary>For a group question, the heading of each item, such as "Clinical trial"; null otherwise.</summary>
    public required string? ItemLabel { get; init; }

    /// <summary>For a group question, its items in order, each with a row per child question; null for any other question.</summary>
    public required IReadOnlyList<RecordCheckAnswersGroupItemDto>? Items { get; init; }

    /// <summary>For a group question, the label of its add-another button, such as "Add another clinical trial"; null otherwise.</summary>
    public required string? AddAnotherLabel { get; init; }

    /// <summary>The control the question uses, which says how to format the answer.</summary>
    public required FormControl Type { get; init; }

    /// <summary>True when the question takes several values rather than one.</summary>
    public required bool Multiple { get; init; }

    /// <summary>
    /// The saved values: at most one for a single-value question, and none when not yet provided.
    /// </summary>
    public required IReadOnlyList<string> Values { get; init; }

    /// <summary>
    /// For a choice question, the chosen options in the order offered, so values can be shown by
    /// label; null for any other question.
    /// </summary>
    public required IReadOnlyList<RecordCheckAnswersOptionDto>? SelectedOptions { get; init; }
}
