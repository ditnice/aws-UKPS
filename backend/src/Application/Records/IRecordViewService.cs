using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records.Dtos.PublishedRecord;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records;

/// <summary>
/// Defines the contract for viewing records.
/// </summary>
public interface IRecordViewService
{
    /// <summary>
    /// Retrieves the data held on the latest published revision of an active or on hold record.
    /// </summary>
    /// <param name="recordId">The unique identifier of the record.</param>
    /// <param name="recordType">The expected type of the record.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a <see cref="Result{TSuccess, TError}"/> object with the published record data
    /// or an error of type <see cref="GetPublishedRecordError"/>.
    /// </returns>
    Task<Result<PublishedRecordDto, GetPublishedRecordError>> GetPublishedRecord(
        int recordId,
        RecordType recordType,
        CancellationToken cancellationToken
    );
}
