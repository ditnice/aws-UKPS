using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;

namespace UKPS.Api.Application.Records;

/// <summary>
/// Defines the contract for record-related operations.
/// </summary>
public interface IRecordService
{
    /// <summary>
    /// Retrieves a paginated list of records based on the specified criteria.
    /// </summary>
    /// <param name="organisationId">The unique identifier of the organisation.</param>
    /// <param name="getRecordsQuery">The query parameters used to filter and paginate records.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> object with a paginated response of records list items
    /// or an error of type <see cref="GetRecordsError"/>.
    /// </returns>
    Task<Result<PaginatedResponseDto<RecordListItemDto>, GetRecordsError>> GetOrganisationRecords(
        int organisationId,
        GetRecordsQueryDto getRecordsQuery,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Discards a draft record identified by its unique ID.
    /// </summary>
    /// <param name="recordId">
    /// The unique identifier of the draft record to discard.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// An <see cref="DiscardRecordError"/> indicating the outcome of the operation.
    /// </returns>
    Task<Result<DiscardRecordError>> DiscardDraftRecord(
        int recordId,
        CancellationToken cancellationToken
    );
}
