using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users.Errors;

namespace UKPS.Api.Application.Users;

/// <summary>
/// Provides operations for managing membership requests.
/// </summary>
public interface IMembershipRequestService
{
    /// <summary>
    /// Approves a membership request.
    /// </summary>
    /// <param name="requestId">
    /// The unique identifier of the membership request to approve.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A result containing an <see cref="ApproveRequestError"/> if the request
    /// could not be approved.
    /// </returns>
    Task<Result<ApproveRequestError>> ApproveRequest(
        int requestId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Rejects a membership request.
    /// </summary>
    /// <param name="requestId">
    /// The unique identifier of the membership request to reject.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A result containing a <see cref="RejectRequestError"/> if the request
    /// could not be rejected.
    /// </returns>
    Task<Result<RejectRequestError>> RejectRequest(
        int requestId,
        CancellationToken cancellationToken
    );
}
