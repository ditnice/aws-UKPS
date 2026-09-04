using System.Diagnostics;
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
[ApiController]
[Route("users")]
public class UserCreationController(IUserAdministrationService userAdministrationService)
    : ControllerBase
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
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
    /// </list>
    /// </returns>
    [ProducesResponseType<RegisterUserConfirmationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserConfirmationDto>> RegisterUser(
        [FromBody] RegisterUserCommandDto registerUserCommandDto,
        CancellationToken cancellationToken
    )
    {
        Result<RegisterUserConfirmationDto, RegisterUserError> result =
            await userAdministrationService.RegisterUser(registerUserCommandDto, cancellationToken);
        return result.Match<ActionResult<RegisterUserConfirmationDto>>(
            x => Ok(x),
            x =>
                x switch
                {
                    RegisterUserError.MissingFields => BadRequest(
                        "Some of the data required is missing."
                    ),
                    _ => throw new UnreachableException(),
                }
        );
    }

    /// <summary>
    /// Retrieves the details of a user by their unique identifier.
    /// </summary>
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
    [HttpGet("registration-requests/{id:int}", Name = nameof(GetUserRegistrationById))]
    public async Task<ActionResult<RegisterUserConfirmationDto>> GetUserRegistrationById(
        int id,
        CancellationToken cancellationToken
    )
    {
        var result = await userAdministrationService.GetUserRegistrationById(id, cancellationToken);

        return result.Match<ActionResult<RegisterUserConfirmationDto>>(
            user => Ok(user),
            error =>
                error switch
                {
                    GetUserDetailsError.IdNotFound => NotFound(),
                    _ => throw new UnreachableException("Unhandled GetUserDetailsError"),
                }
        );
    }

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
    [Authorize]
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
