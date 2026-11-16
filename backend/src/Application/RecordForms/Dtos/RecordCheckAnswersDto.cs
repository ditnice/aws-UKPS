namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>
/// The whole of a record's latest revision for check your answers: every question that applies,
/// grouped by section and task, and every error that would stop it being submitted.
/// </summary>
public sealed record RecordCheckAnswersDto
{
    /// <summary>The form's version. Send it back on submit.</summary>
    public required string FormVersion { get; init; }

    /// <summary>The record the answers belong to.</summary>
    public required int RecordId { get; init; }

    /// <summary>
    /// A hash of the whole revision. Send it back on submit; if anything in the revision has
    /// changed since, the submission is rejected.
    /// </summary>
    public required string RevisionHash { get; init; }

    /// <summary>
    /// Whether the caller may submit the revision in its current state: it is a draft that can be
    /// submitted, and the caller can edit the record. Show Submit only when this is true and
    /// <see cref="Errors"/> is empty.
    /// </summary>
    public required bool CanSubmit { get; init; }

    /// <summary>The sections with a question that applies, in form order.</summary>
    public required IReadOnlyList<RecordCheckAnswersSectionDto> Sections { get; init; }

    /// <summary>
    /// The problems that would stop the record being submitted, in form order, each on a question
    /// in <see cref="Sections"/>.
    /// </summary>
    public required IReadOnlyList<SubmissionErrorDto> Errors { get; init; }
}
