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
    /// Retrieves the distinct company codes on the latest revisions of an organisation's records
    /// that contain the search text, ignoring case, for suggesting while a user types a company
    /// code. Discarded records are excluded.
    /// </summary>
    /// <param name="organisationId">The unique identifier of the organisation.</param>
    /// <param name="query">The search text and the maximum number of suggestions.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// The matching company codes, or a <see cref="GetRecordsError"/> when the organisation does
    /// not exist or the caller cannot read its records.
    /// </returns>
    Task<Result<CompanyCodeSuggestionsDto, GetRecordsError>> GetOrganisationCompanyCodes(
        int organisationId,
        GetCompanyCodeSuggestionsQueryDto query,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Discards a record that has never been submitted, setting the record and its draft
    /// revision to discarded.
    /// </summary>
    /// <param name="recordId">
    /// The unique identifier of the draft record to discard.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{TError}"/>, or a <see cref="DiscardRecordError"/>
    /// describing why the record could not be discarded.
    /// </returns>
    Task<Result<DiscardRecordError>> DiscardDraftRecord(
        int recordId,
        CancellationToken cancellationToken
    );
}
