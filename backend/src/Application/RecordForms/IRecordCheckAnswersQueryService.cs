global using GetCheckAnswersResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.RecordForms.Dtos.RecordCheckAnswersDto,
    UKPS.Api.Application.RecordForms.Errors.GetCheckAnswersError
>;

namespace UKPS.Api.Application.RecordForms;

/// <summary>Serves check your answers for a record.</summary>
public interface IRecordCheckAnswersQueryService
{
    /// <summary>
    /// Loads every question that applies on the record's latest revision with its answer, runs
    /// the checks Submit runs, and returns the revision hash to submit with.
    /// </summary>
    Task<GetCheckAnswersResult> GetCheckAnswers(int recordId, CancellationToken cancellationToken);
}
