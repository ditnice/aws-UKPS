using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
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
    /// Gets the membership request for a user within an organisation.
    /// </summary>
    /// <param name="organisationId">
    /// The unique identifier of the organisation.
    /// </param>
    /// <param name="userId">
    /// The unique identifier of the user.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the request.
    /// </param>
    /// <returns>
    /// A <see cref="UserMembershipRequestDto"/> representing the user's membership
    /// request.
    /// </returns>
    /// <response code="200">
    /// The user membership request was found and returned successfully.
    /// </response>
    /// <response code="403">
    /// The authenticated user is not allowed to access the requested membership
    /// request.
    /// </response>
    /// <response code="404">
    /// The requested user membership request could not be found.
    /// </response>
    [HttpGet]
    [ProducesResponseType<UserMembershipRequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserMembershipRequestDto>> GetUserMembershipRequest(
        int organisationId,
        int userId,
        CancellationToken cancellationToken
    )
    {
        GetUserMembershipRequestResult result =
            await _membershipRequestService.GetUserMembershipRequest(
                organisationId,
                userId,
                cancellationToken
            );

        return result.Match(
            x => Ok(x),
            err =>
                err.Match<ActionResult<UserMembershipRequestDto>>(
                    notFound: _ =>
                        Problem(
                            title: "User membership request not found",
                            detail: "The requested user membership request could not be found.",
                            statusCode: StatusCodes.Status404NotFound
                        ),
                    notAllowed: _ =>
                        Problem(
                            title: "User membership request access denied",
                            detail: "You are not allowed to access the requested user membership request.",
                            statusCode: StatusCodes.Status403Forbidden
                        )
                )
        );
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
    [HttpPatch("approve", Name = nameof(Approve))]
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
    [HttpPatch("reject", Name = nameof(Reject))]
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
