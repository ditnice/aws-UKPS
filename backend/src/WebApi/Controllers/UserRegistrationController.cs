using System.Diagnostics;
using System.Net;
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
[Route("organisations/{organisationId:int}/membership-requests")]
public class UserRegistrationController : ControllerBase
{
    private readonly IUserRegistrationService _membershipRequestService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRegistrationController"/> class.
    /// </summary>
    /// <param name="membershipRequestService">
    /// The service used to approve and reject membership requests.
    /// </param>
    public UserRegistrationController(IUserRegistrationService membershipRequestService)
    {
        _membershipRequestService = membershipRequestService;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="organisationId">
    /// The identifier of the organisation containing the membership request.
    /// </param>
    /// <param name="registerUserCommandDto">
    /// The details required to register the user.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{TValue}"/> containing the registered user's details when the
    /// operation succeeds. Returns:
    /// <list type="bullet">
    /// <item>
    /// <description><c>400 Bad Request</c> if some of the required data is missing.</description>
    /// </item>
    /// <item>
    /// <description><c>404 Not Found</c> if the specified organisation cannot be found or is not active.</description>
    /// </item>
    /// </list>
    /// </returns>
    [AllowAnonymous]
    [ProducesResponseType<RegisterUserConfirmationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost(Name = "RegisterUser")]
    public async Task<ActionResult<RegisterUserConfirmationDto>> RegisterUser(
        int organisationId,
        [FromBody] RegisterUserCommandDto registerUserCommandDto,
        CancellationToken cancellationToken
    )
    {
        Result<RegisterUserConfirmationDto, RegisterUserError> result =
            await _membershipRequestService.RegisterUser(
                organisationId,
                registerUserCommandDto,
                cancellationToken
            );
        return result.Match<ActionResult<RegisterUserConfirmationDto>>(
            x => Ok(x),
            x =>
                x switch
                {
                    RegisterUserError.MissingFields => BadRequest(
                        "Some of the data required is missing."
                    ),
                    RegisterUserError.OrganisationNotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        detail: $"Organisation ID is not found."
                    ),
                    _ => throw new UnreachableException(),
                }
        );
    }

    /// <summary>
    /// Retrieves the details of a user by their unique identifier.
    /// </summary>
    /// <param name="organisationId">
    /// The identifier of the organisation containing the membership request.
    /// </param>
    /// <param name="id">
    /// The unique identifier of the user to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the user's details.
    /// Returns <see cref="OkObjectResult"/> if the user was found,
    /// or <see cref="NotFoundResult"/> if no user exists with the supplied identifier.
    /// </returns>
    /// <response code="200">
    /// The user's details were successfully retrieved.
    /// </response>
    /// <response code="404">
    /// No user was found with the supplied identifier.
    /// </response>
    [ProducesResponseType<RegisterUserConfirmationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}", Name = nameof(GetUserRegistrationById))]
    public async Task<ActionResult<RegisterUserConfirmationDto>> GetUserRegistrationById(
        int organisationId,
        int id,
        CancellationToken cancellationToken
    )
    {
        var result = await _membershipRequestService.GetUserRegistrationById(
            organisationId,
            id,
            cancellationToken
        );

        return result.Match<ActionResult<RegisterUserConfirmationDto>>(
            user => Ok(user),
            error =>
                error switch
                {
                    GetUserDetailsError.IdNotFound => NotFound(),
                    GetUserDetailsError.UserNotAuthorised => Problem(
                        statusCode: (int)HttpStatusCode.Forbidden
                    ),
                    _ => throw new UnreachableException("Unhandled GetUserDetailsError"),
                }
        );
    }

    /// <summary>
    /// Approves the membership request for the specified user within the specified organisation.
    /// </summary>
    /// <param name="organisationId">
    /// The identifier of the organisation containing the membership request.
    /// </param>
    /// <param name="registrationRequestId">
    /// The identifier for the registration request.
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
    [HttpPatch("{registrationRequestId}/approve", Name = nameof(Approve))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Approve(
        int organisationId,
        int registrationRequestId,
        CancellationToken cancellationToken
    )
    {
        var result = await _membershipRequestService.ApproveRequest(
            organisationId,
            registrationRequestId,
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
    /// <param name="registrationRequestId">
    /// The identifier for the registration request.
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
    [HttpPatch("{registrationRequestId}/reject", Name = nameof(Reject))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reject(
        int organisationId,
        int registrationRequestId,
        CancellationToken cancellationToken
    )
    {
        var result = await _membershipRequestService.RejectRequest(
            organisationId,
            registrationRequestId,
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
