using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records.Dtos.RecordDetails;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records;

/// <summary>
/// Defines the contract for viewing records.
/// </summary>
public interface IRecordViewService
{
    /// <summary>
    /// Retrieves the data held on the latest revision of a record.
    /// </summary>
    /// <param name="recordId">The unique identifier of the record.</param>
    /// <param name="recordType">The expected type of the record.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a <see cref="Result{TSuccess, TError}"/> object with the record data
    /// or an error of type <see cref="GetRecordError"/>.
    /// </returns>
    Task<Result<RecordDto, GetRecordError>> GetRecord(
        int recordId,
        RecordType recordType,
        CancellationToken cancellationToken
    );
}
