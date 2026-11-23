using System.Diagnostics;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;

namespace UKPS.Api.Application.RecordForms;

internal sealed class RecordCheckAnswersQueryService(
    RecordFormPageLoader loader,
    RevisionChecker revisionChecker
) : IRecordCheckAnswersQueryService
{
    public async Task<GetCheckAnswersResult> GetCheckAnswers(
        int recordId,
        CancellationToken cancellationToken
    )
    {
        var (form, failure) = await loader.LoadFormAsync(recordId, track: false, cancellationToken);
        if (form is null)
        {
            return GetCheckAnswersResult.Err(
                failure == PageLookupFailure.NotAllowed
                    ? new GetCheckAnswersError.NotAllowed()
                    : new GetCheckAnswersError.NotFound()
            );
        }

        // Check your answers is only for submitting, so it is refused as submit would be.
        GetCheckAnswersError? blocked = revisionChecker.FindSubmitBlocker(form) switch
        {
            null => null,
            SubmitBlocker.CannotSubmit => new GetCheckAnswersError.CannotSubmit(),
            SubmitBlocker.NotSubmittable => new GetCheckAnswersError.NotSubmittable(),
            { } blocker => throw new UnreachableException($"Unhandled submit blocker {blocker}."),
        };
        if (blocked is not null)
        {
            return GetCheckAnswersResult.Err(blocked);
        }

        LoadedRevision revision = await revisionChecker.LoadAsync(form, cancellationToken);
        RevisionCheck check = await revisionChecker.CheckAsync(form, revision, cancellationToken);
        return GetCheckAnswersResult.Ok(
            new RecordCheckAnswersDto
            {
                FormVersion = form.Model.Version,
                RecordId = form.Record.Id,
                OrganisationId = form.Record.OrganisationId,
                RevisionHash = revision.Hash,
                Sections = new RecordAnswersBuilder(
                    form.Model,
                    revision.Answers,
                    revision.Applicability,
                    check.Options
                ).Build(),
                Errors = check.Errors,
            }
        );
    }
}
