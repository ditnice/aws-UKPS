using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Options;

namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// The options of a revision's choice questions, and every error that would stop it being
/// submitted, in form order.
/// </summary>
internal sealed record RevisionCheck(
    IReadOnlyDictionary<string, IReadOnlyList<FormOption>> Options,
    IReadOnlyList<SubmissionErrorDto> Errors
);
