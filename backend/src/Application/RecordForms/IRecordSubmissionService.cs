global using SubmitRecordResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.RecordForms.Errors.SubmitRecordError>;
using UKPS.Api.Application.RecordForms.Dtos;

namespace UKPS.Api.Application.RecordForms;

/// <summary>Submits a record's draft revision.</summary>
public interface IRecordSubmissionService
{
    /// <summary>
    /// Checks the whole draft revision is unchanged since check your answers loaded and passes
    /// every page and cross-page rule, then submits it: a medicine to QA review.
    /// </summary>
    Task<SubmitRecordResult> Submit(
        int recordId,
        SubmitRecordCommand command,
        CancellationToken cancellationToken
    );
}
