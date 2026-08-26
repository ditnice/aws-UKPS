using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Errors;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for updating membership requests.
/// </summary>
[Authorize]
[ApiController]
[Route("membership-requests")]
public class MembershipRequestController : ControllerBase
{
    private readonly IMembershipRequestService _membershipRequestService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MembershipRequestController"/> class.
    /// </summary>
    /// <param name="membershipRequestService">
    /// The service used to update membership requests.
    /// </param>
    public MembershipRequestController(IMembershipRequestService membershipRequestService)
    {
        _membershipRequestService = membershipRequestService;
    }

    /// <summary>
    /// Approves the specified membership request.
    /// </summary>
    /// <param name="membershipRequestId">
    /// The identifier of the membership request to approve.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// The updated membership request when the operation succeeds.
    /// </returns>
    /// <response code="200">
    /// The membership request was successfully approved.
    /// </response>
    /// <response code="403">
    /// The current user is not allowed to update the membership request.
    /// </response>
    /// <response code="404">
    /// The membership request could not be found.
    /// </response>
    [HttpPatch("{membershipRequestId}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Approve(
        int membershipRequestId,
        CancellationToken cancellationToken
    )
    {
        var result = await _membershipRequestService.ApproveRequest(
            membershipRequestId,
            cancellationToken
        );
        return HandleResult(result);
    }

    /// <summary>
    /// Rejects the specified membership request.
    /// </summary>
    /// <param name="membershipRequestId">
    /// The identifier of the membership request to reject.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// The updated membership request when the operation succeeds.
    /// </returns>
    /// <response code="200">
    /// The membership request was successfully rejected.
    /// </response>
    /// <response code="403">
    /// The current user is not allowed to update the membership request.
    /// </response>
    /// <response code="404">
    /// The membership request could not be found.
    /// </response>
    [HttpPatch("{membershipRequestId}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reject(
        int membershipRequestId,
        CancellationToken cancellationToken
    )
    {
        var result = await _membershipRequestService.RejectRequest(
            membershipRequestId,
            cancellationToken
        );
        return HandleResult(result);
    }

    private ActionResult HandleResult<T>(Result<T> result)
        where T : IMembershipRequestUpdateError
    {
        ActionResult HandleError(IMembershipRequestUpdateError error)
        {
            return error.Match(
                notAllowed: _ =>
                    Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        title: "Membership request update not allowed",
                        detail: "You are not allowed to update this membership request."
                    ),
                requestNotFound: _ =>
                    Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Membership request not found",
                        detail: "The requested membership request could not be found."
                    )
            );
        }

        return result.Match(Ok, err => HandleError(err));
    }
}
