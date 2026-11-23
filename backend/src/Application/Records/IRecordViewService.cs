using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records;

/// <summary>
/// Defines the contract for viewing records.
/// </summary>
public interface IRecordViewService
{
    /// <summary>
    /// Retrieves a record's status, permitted actions and identifying details from its latest
    /// revision, without its answers.
    /// </summary>
    /// <param name="recordId">The unique identifier of the record.</param>
    /// <param name="recordType">The expected type of the record.</param>
    /// <param name="organisationId">The organisation the record is expected to belong to.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task<Result<RecordOverviewDto, GetRecordError>> GetRecordOverview(
        int recordId,
        RecordType recordType,
        int organisationId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Retrieves a record's overview and every question that applies on its latest revision with
    /// its answer, as check your answers shows them but without checking them.
    /// </summary>
    /// <param name="recordId">The unique identifier of the record.</param>
    /// <param name="recordType">The expected type of the record.</param>
    /// <param name="organisationId">The organisation the record is expected to belong to.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task<Result<RecordDetailDto, GetRecordError>> GetRecordDetail(
        int recordId,
        RecordType recordType,
        int organisationId,
        CancellationToken cancellationToken
    );
}
