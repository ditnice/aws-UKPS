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

    /// <summary>The organisation that owns the record; used for links back to the records list.</summary>
    public required int OrganisationId { get; init; }

    /// <summary>
    /// A hash of the whole revision. Send it back on submit; if anything in the revision has
    /// changed since, the submission is rejected.
    /// </summary>
    public required string RevisionHash { get; init; }

    /// <summary>The sections with a question that applies, in form order.</summary>
    public required IReadOnlyList<RecordCheckAnswersSectionDto> Sections { get; init; }

    /// <summary>
    /// The problems that would stop the record being submitted, in form order, each on a question
    /// in <see cref="Sections"/>. The record can be submitted when this is empty.
    /// </summary>
    public required IReadOnlyList<SubmissionErrorDto> Errors { get; init; }
}
