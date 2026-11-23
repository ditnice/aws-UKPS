using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A question that applies and its answer, shown as a summary row.</summary>
public sealed record RecordAnswersQuestionDto
{
    /// <summary>
    /// The question ID, which errors are keyed by. A group's question has an ID local to the group.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>The page the question is on, for its Change link.</summary>
    public required string PageId { get; init; }

    /// <summary>The question text.</summary>
    public required string Label { get; init; }

    /// <summary>The control the question uses, which says how to format the answer.</summary>
    public required FormControl Type { get; init; }

    /// <summary>True when the question takes several values rather than one.</summary>
    public required bool Multiple { get; init; }

    /// <summary>
    /// The saved values: at most one for a single-value question, and none when not yet provided
    /// or for a group.
    /// </summary>
    public required IReadOnlyList<string> Values { get; init; }

    /// <summary>
    /// For a choice question, the chosen options in the order offered, so values can be shown by
    /// label; null for any other question.
    /// </summary>
    public required IReadOnlyList<RecordAnswersOptionDto>? SelectedOptions { get; init; }

    /// <summary>
    /// For a group, the heading of each item, numbered from 1, such as "Clinical trial"; null for
    /// any other question.
    /// </summary>
    public required string? ItemLabel { get; init; }

    /// <summary>For a group, its items in order; null for any other question.</summary>
    public required IReadOnlyList<RecordAnswersGroupItemDto>? Items { get; init; }

    /// <summary>
    /// For a group, the text of the button that adds an item from check your answers, such as
    /// "Add clinical trial"; null for any other question.
    /// </summary>
    public required string? CheckAnswersAddLabel { get; init; }
}
