using System.Diagnostics;
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
    [HttpPost("onboard")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> OnboardUser(
        [FromBody] OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        Result<OnboardUserError> result = await userAdministrationService.OnboardUser(
            command,
            cancellationToken
        );
        return result.Match<ActionResult>(
            Created,
            err =>
                err switch
                {
                    OnboardUserError.NotAllowed => Problem(
                        title: "Forbidden",
                        detail: "You do not have permission to perform this action.",
                        statusCode: StatusCodes.Status403Forbidden
                    ),
                    _ => throw new UnreachableException(
                        $"Unhandled {nameof(OnboardUserError)} variant."
                    ),
                }
        );
    }
}
