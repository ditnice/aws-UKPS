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
    /// <param name="registerUserDto">
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
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserDetailsDto>> RegisterUser(
        [FromBody] RegisterUserDto registerUserDto,
        CancellationToken cancellationToken
    )
    {
        Result<RegisterUserDetailsDto, RegisterUserError> result =
            await userAdministrationService.RegisterUser(registerUserDto, cancellationToken);
        return result.Match<ActionResult<RegisterUserDetailsDto>>(
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
    /// Returns <see cref="OkResult"/> if the user was successfully onboarded,
    /// or <see cref="ForbidResult"/> if the current user is not permitted to
    /// onboard users.
    /// </returns>
    /// <response code="200">
    /// The user was successfully onboarded.
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> OnboardUser(
        [FromBody] OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        Result<OnboardUserError> result = await userAdministrationService.OnboardUser(
            command,
            cancellationToken
        );
        return result.Match(
            Created,
            err =>
                err.Match<ActionResult>(
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
