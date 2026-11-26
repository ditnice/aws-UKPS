using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.WebApi.CustomResponses;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Controller responsible for managing user-related operations.
/// </summary>
[Authorize]
[ApiController]
[Route("users")]
public class UserController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Gets the information for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the request.
    /// </param>
    /// <response code="200">
    /// Returns the information for the currently authenticated user.
    /// </response>
    /// <returns>
    /// The current user's information.
    /// </returns>
    [ProducesResponseType<CurrentUserInformationDto>(StatusCodes.Status200OK)]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserInformationDto>> GetCurrentUser(
        CancellationToken cancellationToken
    )
    {
        return await userService.GetCurrentUser(cancellationToken);
    }

    /// <summary>
    /// Retrieves a paginated list of users based on the specified query parameters.
    /// </summary>
    /// <param name="getUsersQuery">The query parameters for retrieving users, including organisation ID, page, page size, status, role, and email filters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A paginated response containing a list of users if successful, or an appropriate error response if the request fails.
    /// </returns>
    /// <response code="200">Returns the paginated list of users.</response>
    /// <response code="400">Returned if the query parameters are invalid or the organisation is not found.</response>
    /// <response code="403">Returned if the caller is not authorised to view the users of the organisation.</response>
    [HttpGet(Name = nameof(GetUsers))]
    [ProducesResponseType<PaginatedResponseDto<UserListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResponseDto<UserListItemDto>>> GetUsers(
        [FromQuery] GetUsersQueryDto? getUsersQuery,
        CancellationToken cancellationToken
    )
    {
        if (getUsersQuery is null)
        {
            return BadRequest();
        }

        var result = await userService.GetUsers(getUsersQuery, cancellationToken);

        return result.Match<ActionResult<PaginatedResponseDto<UserListItemDto>>>(
            items => Ok(items),
            error =>
                error switch
                {
                    GetUsersError.OrganisationNotFound => BadRequest("Organisation not found."),
                    GetUsersError.NotAllowed => Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        title: "Forbidden",
                        detail: "You are not authorised to view users."
                    ),
                    _ => throw new UnreachableException("Unhandled GetUsersError variant."),
                }
        );
    }

    /// <summary>
    /// Retrieves a user's details along with their role within the specified organisation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="organisationId">
    /// The unique identifier of the organisation to read the user's membership from. A user may
    /// belong to several organisations, so their role is resolved against this organisation.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="UserInformationDto"/> describing the user and their organisation membership if
    /// successful, or an appropriate error response if the request fails.
    /// </returns>
    /// <response code="200">Returns the user's details and their role within the organisation.</response>
    /// <response code="404">The user does not exist in this organisation or is not accessible to the caller (ADR-004).</response>
    [HttpGet(
        "{userId:int}/organisations/{organisationId:int}",
        Name = nameof(GetUserDetailsWithinOrganisation)
    )]
    [ProducesResponseType<UserInformationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInformationDto>> GetUserDetailsWithinOrganisation(
        int userId,
        int organisationId,
        CancellationToken cancellationToken
    )
    {
        var result = await userService.GetUserDetailsWithinOrganisation(
            userId,
            organisationId,
            cancellationToken
        );

        return result.Match<ActionResult<UserInformationDto>>(
            user => Ok(user),
            error =>
                error switch
                {
                    GetUsersError.OrganisationNotFound
                    or GetUsersError.UserNotFound
                    or GetUsersError.NotAllowed => NotFound(),
                    _ => throw new UnreachableException("Unhandled GetUsersError variant."),
                }
        );
    }

    /// <summary>
    /// Updates the details of the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose details are being updated.</param>
    /// <param name="command">The updated user details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// Returns <see cref="UserDetailsDto"/> with the updated user details when the operation
    /// succeeds (200 OK).
    /// Returns a bad request response (400 Bad Request) when the supplied user details are invalid.
    /// Returns a not found response (404 Not Found) when the specified user does not exist or the
    /// caller is not authorised to update them. The two cases are deliberately indistinguishable
    /// so that user identifiers cannot be enumerated (see ADR-004).
    /// </returns>
    /// <response code="200">The user's details were successfully updated.</response>
    /// <response code="400">The supplied user details are invalid.</response>
    /// <response code="404">The specified user does not exist or is not accessible to the caller.</response>
    /// <response code="409">The request conflicts with the existing data such as another users email.</response>
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPatch("{userId:int}", Name = nameof(UpdateUserDetails))]
    public async Task<ActionResult<UserDetailsDto>> UpdateUserDetails(
        [FromRoute] int userId,
        [FromBody] UpdateUserDetailsCommand command,
        CancellationToken cancellationToken
    )
    {
        Result<UserDetailsDto, UpdateUserDetailsError> result = await userService.UpdateUserDetails(
            userId,
            command,
            cancellationToken
        );

        return result.Match(
            x => Ok(x),
            err =>
            {
                // Unauthorised and missing users share a response so that user identifiers
                // cannot be enumerated (ADR-004).
                return err.Match<ActionResult<UserDetailsDto>>(
                    unauthorised: () => UserNotFound(),
                    userDoesNotExist: () => UserNotFound(),
                    validationError: error =>
                        ValidationProblem(
                            new ValidationProblemDetails(
                                new Dictionary<string, string[]>(StringComparer.Ordinal)
                                {
                                    [nameof(command.Value)] = [error.ErrorMessage],
                                }
                            )
                        )
                );
            }
        );
    }

    /// <summary>
    /// Updates the email address of the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose email address is to be updated.</param>
    /// <param name="command">The command containing the new email address.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the request.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> that is:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c> with the updated <see cref="UserDetailsDto"/> if the email was updated successfully.</description></item>
    /// <item><description><c>400 Bad Request</c> with a <see cref="ValidationProblemDetails"/> if the request body fails validation.</description></item>
    /// <item><description><c>404 Not Found</c> if the user does not exist or the caller is not permitted to access it.</description></item>
    /// <item><description><c>409 Conflict</c> if the specified email is already in use by another user.</description></item>
    /// </list>
    /// </returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPatch("{userId:int}/email", Name = nameof(UpdateUserEmail))]
    public async Task<ActionResult> UpdateUserEmail(
        [FromRoute] int userId,
        [FromBody] UpdateUserEmailCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await userService.UpdateUserEmail(userId, command, cancellationToken);

        return result.Match(
            Ok,
            err =>
                err.Match(
                    unauthorised: () => UserNotFound(),
                    userDoesNotExist: () => UserNotFound(),
                    conflictingEmail: () =>
                        Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Conflict",
                            detail: "The specified email conflicts with an existing email."
                        ),
                    () =>
                        ValidationProblem(
                            new ValidationProblemDetails(
                                new Dictionary<string, string[]>(StringComparer.Ordinal)
                                {
                                    [nameof(command.Value)] =
                                    [
                                        "New email cannot be the same as the current email.",
                                    ],
                                }
                            )
                        ),
                    concurrentChangeDetected: () =>
                        Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Conflict",
                            detail: "The user was modified by another request. Please retry."
                        )
                )
        );
    }

    /// <summary>
    /// Resends the email verification message to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to resend the verification email to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// An empty <c>200 OK</c> response when the verification email has been queued for sending.
    /// </returns>
    /// <response code="200">The verification email was successfully resent.</response>
    /// <response code="404">No user exists with the specified <paramref name="userId"/>.</response>
    /// <response code="409">
    /// The request could not be completed because there is no pending verification for the user,
    /// or because a concurrent modification was detected. Retry may succeed in the latter case.
    /// </response>
    [HttpPost("{userId:int}/email/resend", Name = nameof(ResendVerificationEmail))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> ResendVerificationEmail(
        [FromRoute] int userId,
        CancellationToken cancellationToken
    )
    {
        ResendVerificationEmailResult result = await userService.ResendVerificationEmail(
            userId,
            cancellationToken
        );

        var notFoundResult = Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "User not found",
            detail: $"No user with id '{userId}' exists."
        );
        return result.Match(
            Ok,
            err =>
                err.Match<ActionResult>(
                    notFound: _ => notFoundResult,
                    noPendingVerification: _ =>
                        Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "No pending verification",
                            detail: "User has no pending email verification to resend."
                        ),
                    _ =>
                        Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Email already verified",
                            detail: "The user has already verified this email."
                        ),
                    forbidden: _ => notFoundResult,
                    concurrencyDetected: _ =>
                        Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Concurrency conflict",
                            detail: "The user was modified by another request. Please try again."
                        )
                )
        );
    }

    /// <summary>
    /// Verifies a user's pending email address using a verification token.
    /// </summary>
    /// <param name="userId">The ID of the user whose pending email address is being verified.</param>
    /// <param name="command">The command containing the verification token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An empty 200 response when the pending email address has been verified.</returns>
    /// <response code="200">The pending email address was verified successfully.</response>
    /// <response code="400">The verification token provided is invalid.</response>
    /// <response code="403">The caller is not authorised to verify the email address for this user.</response>
    /// <response code="404">The user does not exist.</response>
    /// <response code="409">The user has no pending email address, or the pending email address has already been verified.</response>
    [HttpPost("{userId:int}/email/verify", Name = nameof(VerifyPendingEmailAddress))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> VerifyPendingEmailAddress(
        [FromRoute] int userId,
        [FromBody] VerifyPendingEmailAddressCommand command,
        CancellationToken cancellationToken
    )
    {
        VerifyPendingEmailAddressResult result = await userService.VerifyPendingEmailAddress(
            userId,
            command,
            cancellationToken
        );

        return result.Match(
            Ok,
            err =>
                err.Match<ActionResult>(
                    userNotFound: () =>
                        Problem(
                            title: "User not found",
                            detail: $"No user with ID {userId} exists.",
                            statusCode: StatusCodes.Status404NotFound
                        ),
                    unauthorised: () =>
                        Problem(
                            title: "Unauthorised",
                            detail: "You are not authorised to verify the email address for this user.",
                            statusCode: StatusCodes.Status403Forbidden
                        ),
                    noPendingEmailToVerified: () =>
                        Problem(
                            title: "No pending email",
                            detail: "This user has no pending email address awaiting verification.",
                            statusCode: StatusCodes.Status409Conflict
                        ),
                    emailAlreadyVerified: () =>
                        Problem(
                            title: "Email already verified",
                            detail: "The pending email address has already been verified.",
                            statusCode: StatusCodes.Status409Conflict
                        ),
                    invalidTokenProvided: () =>
                        Problem(
                            title: "Invalid verification token",
                            detail: "The verification token provided is invalid or has expired.",
                            statusCode: StatusCodes.Status400BadRequest
                        ),
                    concurrentChangeDetected: () =>
                        Problem(
                            title: "Email address change could not be completed",
                            detail: "A concurrent change was detected. Please try changing your email address again through the portal.",
                            statusCode: StatusCodes.Status409Conflict
                        )
                )
        );
    }

    /// <summary>
    /// Cancels a user's unverified pending email change.
    /// </summary>
    /// <param name="userId">The ID of the user whose pending email change is being cancelled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An empty 200 response when the pending email change has been cancelled.</returns>
    /// <response code="200">The pending email change was cancelled successfully.</response>
    /// <response code="404">The specified user does not exist or is not accessible to the caller.</response>
    /// <response code="409">The user has no pending email change or a concurrent change was detected.</response>
    [HttpDelete("{userId:int}/email/pending", Name = nameof(CancelPendingEmailChange))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CancelPendingEmailChange(
        [FromRoute] int userId,
        CancellationToken cancellationToken
    )
    {
        Result<CancelPendingEmailChangeError> result = await userService.CancelPendingEmailChange(
            userId,
            cancellationToken
        );

        return result.Match(
            Ok,
            err =>
                err.Match<ActionResult>(
                    unauthorised: () => UserNotFound(),
                    userDoesNotExist: () => UserNotFound(),
                    noPendingEmailToCancel: () =>
                        Problem(
                            title: "No pending email",
                            detail: "This user has no pending email change to cancel.",
                            statusCode: StatusCodes.Status409Conflict
                        ),
                    concurrentChangeDetected: () =>
                        Problem(
                            title: "Email address change could not be cancelled",
                            detail: "A concurrent change was detected. Please try again.",
                            statusCode: StatusCodes.Status409Conflict
                        )
                )
        );
    }

    /// <summary>
    /// Removes access and anonymises the user's profile and linked approved registration requests.
    /// Personal details in user-change audit history are cleared. Records created by the user are
    /// kept; other registration requests, communications, logs and backups are not deleted.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to remove.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <response code="200">The user was removed. Returns the name they now appear under.</response>
    /// <response code="400">One of the user's organisation memberships is not in a state that allows removal, or removal would leave no eligible champion.</response>
    /// <response code="403">The caller attempted to remove themselves.</response>
    /// <response code="404">The specified user does not exist or is not accessible to the caller.</response>
    [ProducesResponseType<RemovedUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{userId:int}", Name = nameof(RemoveUser))]
    public async Task<ActionResult<RemovedUserDto>> RemoveUser(
        [FromRoute] int userId,
        CancellationToken cancellationToken
    )
    {
        var result = await userService.RemoveUser(userId, cancellationToken);

        return result.Match<ActionResult<RemovedUserDto>>(
            removedUser => Ok(removedUser),
            err =>
                err.Match<ActionResult<RemovedUserDto>>(
                    // Conceal inaccessible users using the same response as missing users (ADR-004).
                    notAllowed: () => UserNotFound(),
                    userNotFound: () => UserNotFound(),
                    cannotRemoveSelf: () =>
                        Problem(
                            statusCode: StatusCodes.Status403Forbidden,
                            title: "Forbidden",
                            detail: "You cannot remove yourself."
                        ),
                    notAllowedInCurrentState: x =>
                        Problem(
                            title: "Invalid membership state",
                            statusCode: StatusCodes.Status400BadRequest,
                            detail: $"The user's organisation membership is not in a state that allows this action. Current state [{x.TransitionResult.CurrentState}]"
                        ),
                    lastRemainingChampion: _ =>
                        Problem(
                            title: "Last remaining champion",
                            statusCode: StatusCodes.Status400BadRequest,
                            detail: "This action would leave an organisation without a champion user."
                        )
                )
        );
    }

    private ObjectResult UserNotFound() =>
        this.NotFoundProblem("The specified user could not be found.");
}
