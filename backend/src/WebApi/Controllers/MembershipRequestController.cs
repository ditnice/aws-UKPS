using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Errors;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for approving and rejecting membership requests.
/// </summary>
[Authorize]
[ApiController]
[Route("organisations/{organisationId:int}/users/{userId:int}/membership-requests")]
public class MembershipRequestController : ControllerBase
{
    private readonly IMembershipRequestService _membershipRequestService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MembershipRequestController"/> class.
    /// </summary>
    /// <param name="membershipRequestService">
    /// The service used to approve and reject membership requests.
    /// </param>
    public MembershipRequestController(IMembershipRequestService membershipRequestService)
    {
        _membershipRequestService = membershipRequestService;
    }

    /// <summary>
    /// Approves the membership request for the specified user within the specified organisation.
    /// </summary>
    /// <param name="organisationId">
    /// The identifier of the organisation containing the membership request.
    /// </param>
    /// <param name="userId">
    /// The identifier of the user associated with the membership request.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> indicating whether the membership request was successfully approved.
    /// </returns>
    /// <response code="200">
    /// The membership request was successfully approved.
    /// </response>
    /// <response code="403">
    /// The current user is not allowed to approve the membership request.
    /// </response>
    /// <response code="404">
    /// The membership request could not be found.
    /// </response>
    [HttpPatch("approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Approve(
        int organisationId,
        int userId,
        CancellationToken cancellationToken
    )
    {
        var result = await _membershipRequestService.ApproveRequest(
            organisationId,
            userId,
            cancellationToken
        );

        return HandleResult(result);
    }

    /// <summary>
    /// Rejects the membership request for the specified user within the specified organisation.
    /// </summary>
    /// <param name="organisationId">
    /// The identifier of the organisation containing the membership request.
    /// </param>
    /// <param name="userId">
    /// The identifier of the user associated with the membership request.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> indicating whether the membership request was successfully rejected.
    /// </returns>
    /// <response code="200">
    /// The membership request was successfully rejected.
    /// </response>
    /// <response code="403">
    /// The current user is not allowed to reject the membership request.
    /// </response>
    /// <response code="404">
    /// The membership request could not be found.
    /// </response>
    [HttpPatch("reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reject(
        int organisationId,
        int userId,
        CancellationToken cancellationToken
    )
    {
        var result = await _membershipRequestService.RejectRequest(
            organisationId,
            userId,
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
