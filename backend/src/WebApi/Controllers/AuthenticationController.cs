using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Application.AuthorisationAdministration;
using UKPS.Api.Application.Common;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides API endpoints for authenticating users.
/// </summary>
/// <remarks>
/// This controller handles user login requests and issues authentication
/// credentials when valid credentials are provided.
/// </remarks>
[ApiController]
[Route("auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthorisationAdministrationService _authorisationAdministrationService;
    private readonly ProblemDetails _setupTokenExpiredDetails = new ProblemDetails
    {
        Title = "Setup token has expired.",
        Detail =
            "The setup token has expired and can no longer be used. Request a new setup token and try again.",
        Status = StatusCodes.Status401Unauthorized,
    };
    private readonly ProblemDetails _setupTokenNotFound = new ProblemDetails
    {
        Title = "Setup token not found.",
        Detail = "The supplied setup token does not exist.",
        Status = StatusCodes.Status404NotFound,
    };
    private readonly ProblemDetails _setupTokenConsumed = new ProblemDetails
    {
        Title = "Setup token has already been used.",
        Detail = "The setup token has already been consumed and cannot be used again.",
        Status = StatusCodes.Status401Unauthorized,
    };

    /// <summary>
    /// Initialises a new instance of the <see cref="AuthenticationController"/> class.
    /// </summary>
    /// <param name="authenticationService">
    /// The service responsible for handling authentication operations.
    /// </param>
    /// <param name="authorisationAdministrationService">
    /// The service responsible for managing authorisation administration operations,
    /// such as user onboarding, setup token validation, and password management.
    /// </param>
    public AuthenticationController(
        IAuthenticationService authenticationService,
        IAuthorisationAdministrationService authorisationAdministrationService
    )
    {
        _authenticationService = authenticationService;
        _authorisationAdministrationService = authorisationAdministrationService;
    }

    /// <summary>
    /// Authenticates a user and creates an authentication session.
    /// </summary>
    /// <param name="request">
    /// The login credentials supplied by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// An <see cref="OkResult"/> when authentication succeeds, or an
    /// <see cref="UnauthorizedResult"/> when the supplied credentials are invalid.
    /// </returns>
    /// <response code="200">
    /// The user was successfully authenticated and an authentication cookie was created.
    /// </response>
    /// <response code="400">
    /// The supplied request was invalid.
    /// </response>
    /// <response code="401">
    /// The supplied credentials were invalid.
    /// </response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        ActionResult HandleLoginSuccess(AuthenticationCredentialsDto dto)
        {
            Response.Cookies.Append(
                "access_token",
                dto.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // HTTPS only
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15),
                }
            );

            return Ok();
        }

        ActionResult HandleLoginError(LoginError error)
        {
            return error switch
            {
                LoginError.Unauthorised => Unauthorized(),
                LoginError.Challenge c => Unauthorized(c),
                _ => throw new UnreachableException($"Unhandled {nameof(LoginError)} variant."),
            };
        }

        ArgumentNullException.ThrowIfNull(request);

        return (await _authenticationService.Login(request, cancellationToken)).Match(
            HandleLoginSuccess,
            HandleLoginError
        );
    }

    /// <summary>
    /// Validates whether a user setup token is valid and can be used to complete the user setup process.
    /// </summary>
    /// <param name="setupToken">
    /// The unique identifier of the setup token to validate.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see cref="OkResult"/> if the setup token is valid.
    /// Returns <see cref="UnauthorizedObjectResult"/> if the setup token has expired or has already been consumed.
    /// Returns <see cref="NotFoundObjectResult"/> if the specified setup token does not exist.
    /// </returns>
    /// <response code="200">
    /// The setup token is valid and can be used.
    /// </response>
    /// <response code="401">
    /// The setup token has expired or has already been consumed.
    /// </response>
    /// <response code="404">
    /// The specified setup token does not exist.
    /// </response>
    [HttpGet("validate-setup-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ValidateSetupToken(
        [Required] Guid setupToken,
        CancellationToken cancellationToken
    )
    {
        Result<SetupTokenValidationError> result =
            await _authorisationAdministrationService.Validate(setupToken, cancellationToken);

        return result.Match(
            Ok,
            err =>
                err.Match<ActionResult>(
                    expired: _ => Unauthorized(_setupTokenExpiredDetails),
                    doesNotExist: _ => NotFound(_setupTokenNotFound),
                    consumed: _ => Unauthorized(_setupTokenConsumed)
                )
        );
    }

    /// <summary>
    /// Completes the setup process for a user account using a valid setup token.
    /// </summary>
    /// <param name="setupUserCommand">
    /// The command containing the setup token and user details required to initialise the account.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> indicating whether the user setup was completed successfully.
    /// Returns <see cref="StatusCodes.Status200OK"/> when setup completes successfully.
    /// Returns <see cref="StatusCodes.Status400BadRequest"/> when the supplied password does not
    /// meet the required standards.
    /// Returns <see cref="StatusCodes.Status401Unauthorized"/> when the setup token has expired
    /// or has already been consumed.
    /// Returns <see cref="StatusCodes.Status404NotFound"/> when the setup token cannot be found.
    /// </returns>
    [HttpPost("setup-user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetupUser(
        SetupUserCommand setupUserCommand,
        CancellationToken cancellationToken
    )
    {
        Result<UserSetupError> result = await _authorisationAdministrationService.SetupUser(
            setupUserCommand,
            cancellationToken
        );

        return result.Match(
            Ok,
            err =>
            {
                return err.Match<ActionResult>(
                    consumed: () => Unauthorized(_setupTokenConsumed),
                    invalidPassword: () =>
                        BadRequest("The password does not meet the expected standards."),
                    expired: () => Unauthorized(_setupTokenExpiredDetails),
                    doesNotExist: () => NotFound(_setupTokenNotFound)
                );
            }
        );
    }
}
