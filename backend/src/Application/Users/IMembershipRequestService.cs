using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users.Errors;

namespace UKPS.Api.Application.Users;

/// <summary>
/// Provides operations for managing membership requests.
/// </summary>
public interface IMembershipRequestService
{
    /// <summary>
    /// Approves a membership request for the specified user and organisation.
    /// </summary>
    /// <param name="organisationId">
    /// The unique identifier of the organisation associated with the membership request.
    /// </param>
    /// <param name="userId">
    /// The unique identifier of the user whose membership request is being approved.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an
    /// <see cref="ApproveRequestError"/> if the request could not be approved.
    /// </returns>
    Task<Result<ApproveRequestError>> ApproveRequest(
        int organisationId,
        int userId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Rejects a membership request for the specified user and organisation.
    /// </summary>
    /// <param name="organisationId">
    /// The unique identifier of the organisation associated with the membership request.
    /// </param>
    /// <param name="userId">
    /// The unique identifier of the user whose membership request is being rejected.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains a
    /// <see cref="RejectRequestError"/> if the request could not be rejected.
    /// </returns>
    Task<Result<RejectRequestError>> RejectRequest(
        int organisationId,
        int userId,
        CancellationToken cancellationToken
    );
}
