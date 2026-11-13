using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;

namespace UKPS.Api.Application.Records;

/// <summary>
/// Defines the contract for changing a record's status.
/// </summary>
public interface IRecordStatusService
{
    /// <summary>
    /// Puts a record on hold, resumes it or archives it, and records the change in its status
    /// history and timeline.
    /// </summary>
    /// <param name="recordId">The record to change.</param>
    /// <param name="organisationId">The organisation the record is expected to belong to.</param>
    /// <param name="change">The status change to make.</param>
    /// <param name="command">Why the status is changing.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A successful <see cref="Result{TError}"/>, or a <see cref="ChangeRecordStatusError"/>
    /// describing why the status could not be changed.
    /// </returns>
    Task<Result<ChangeRecordStatusError>> ChangeStatus(
        int recordId,
        int organisationId,
        RecordStatusChangeAction change,
        ChangeRecordStatusCommand command,
        CancellationToken cancellationToken
    );
}
