namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A problem that stops a record being submitted, and the question it is on.</summary>
public sealed record SubmissionErrorDto
{
    /// <summary>The question to highlight, which is also its key in <c>errors</c>.</summary>
    public required string QuestionId { get; init; }

    /// <summary>The page the question is on, to link to.</summary>
    public required string PageId { get; init; }

    /// <summary>The task the page belongs to.</summary>
    public required string TaskId { get; init; }

    /// <summary>What is wrong and how to fix it.</summary>
    public required string Message { get; init; }
}
