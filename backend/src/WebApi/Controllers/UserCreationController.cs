using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for creating new user accounts and managing the user
/// onboarding process.
/// </summary>
[Authorize]
[ApiController]
[Route("users")]
public class UserCreationController(IUserAdministrationService userAdministrationService)
    : ControllerBase
{
    /// <summary>
    /// Creates a new user account and initiates the onboarding process.
    /// </summary>
    /// <param name="command">
    /// The details of the user to onboard.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> indicating the outcome of the operation.
    /// Returns <see cref="CreatedResult"/> containing the new user's identifier
    /// if the user was successfully onboarded, or a forbidden response if the
    /// current user is not permitted to onboard users.
    /// </returns>
    /// <response code="201">
    /// The user was successfully onboarded. The response contains the
    /// identifier of the newly created user.
    /// </response>
    /// <response code="400">
    /// The request was invalid.
    /// </response>
    /// <response code="403">
    /// The current user does not have permission to onboard users.
    /// </response>
    /// <response code="409">
    /// A user with the supplied username already exists.
    /// </response>
    [HttpPost("onboard")]
    [ProducesResponseType<OnboardedUserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OnboardedUserDto>> OnboardUser(
        [FromBody] OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        Result<int, OnboardUserError> result = await userAdministrationService.OnboardUser(
            command,
            cancellationToken
        );
        return result.Match<ActionResult<OnboardedUserDto>>(
            userId =>
                Created(
                    new Uri($"/users/{userId}/organisations/{command.OrganisationId}"),
                    new OnboardedUserDto { UserId = userId }
                ),
            err =>
                err.Match<ActionResult<OnboardedUserDto>>(
                    usernameAlreadyExists: _ =>
                        Conflict("A user with the specified username already exists."),
                    invalidOrganisation: _ =>
                        BadRequest("The specified organisation does not exist."),
                    notAllowed: _ =>
                        Problem(
                            title: "Forbidden",
                            detail: "You do not have permission to perform this action.",
                            statusCode: StatusCodes.Status403Forbidden
                        )
                )
        );
    }
}
