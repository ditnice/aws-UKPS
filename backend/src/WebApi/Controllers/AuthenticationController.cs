using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.WebApi.CustomResponses;
using SetupUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.MultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.Authentication.Errors.UserSetupError
>;

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
    private const string CsrfCookieName = "csrf_token";
    private const string CsrfHeaderName = "X-CSRF-Token";
    private const string RefreshCookieName = "refresh_token";

    private readonly ILoginService _loginService;
    private readonly IIdentityAdministrationService _authorisationAdministrationService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly LinkGenerator _linkGenerator;
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
    /// <param name="loginService">
    /// The service responsible for handling login and associated requests.
    /// </param>
    /// <param name="authorisationAdministrationService">
    /// The service responsible for managing authorisation administration operations,
    /// such as user onboarding, setup token validation, and password management.
    /// </param>
    /// <param name="dateTimeProvider"></param>
    /// <param name="linkGenerator"></param>
    public AuthenticationController(
        ILoginService loginService,
        IIdentityAdministrationService authorisationAdministrationService,
        IDateTimeProvider dateTimeProvider,
        LinkGenerator linkGenerator
    )
    {
        _loginService = loginService;
        _authorisationAdministrationService = authorisationAdministrationService;
        _dateTimeProvider = dateTimeProvider;
        _linkGenerator = linkGenerator;
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
    [ProducesResponseType<AuthenticationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<AuthenticationProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        return (await _loginService.Login(request, cancellationToken)).Match(
            HandleLoginSuccess,
            HandleLoginError
        );
    }

    /// <summary>
    /// Refreshes the authentication tokens using the supplied refresh token.
    /// </summary>
    /// <remarks>
    /// The CSRF token must be supplied in the <c> X-CSRF-Token </c> request header.
    /// The value of this header must match the <c> csrf_token </c> cookie sent with the request.
    /// Requests with a missing or invalid CSRF token are rejected with an unauthorized response.
    /// </remarks>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing one of the following responses:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="StatusCodes.Status200OK"/> when the refresh token is valid and new authentication
    /// credentials have been successfully issued.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="StatusCodes.Status400BadRequest"/> when the refresh token request is invalid or
    /// cannot be processed.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="StatusCodes.Status401Unauthorized"/> when the CSRF token is missing or invalid,
    /// or when the supplied authentication credentials cannot be authenticated.
    /// </description>
    /// </item>
    /// </list>
    /// </returns>
    /// <response code="200">The refresh token was successfully exchanged for new authentication credentials.</response>
    /// <response code="400">The refresh token request was invalid or could not be processed.</response>
    /// <response code="401">The CSRF token was missing or invalid, or the supplied authentication credentials were unauthorized.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<AuthenticationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<AuthenticationProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        if (!PassesCsrfValidation())
        {
            return Unauthorized(
                new ProblemDetails
                {
                    Title = "CSRF validation failed",
                    Detail =
                        "The request could not be authenticated because the CSRF token was missing or invalid.",
                }
            );
        }
        var command = new RefreshAuthenticationTokenCommand
        {
            RefreshToken = Request.Cookies[RefreshCookieName] ?? string.Empty,
        };
        return (await _loginService.RefreshAuthenticationToken(command, cancellationToken)).Match(
            HandleLoginSuccess,
            HandleLoginError
        );
    }

    /// <summary>
    /// Completes a multi-factor authentication challenge for an existing authentication session.
    /// </summary>
    /// <param name="command">
    /// The authentication session identifier and multi-factor authentication code
    /// required to complete the authentication challenge.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// An <see cref="OkResult"/> when the multi-factor authentication challenge is
    /// successfully completed, or an <see cref="UnauthorizedResult"/> when the
    /// authentication session or verification code is invalid.
    /// </returns>
    /// <response code="200">
    /// The multi-factor authentication challenge was successfully completed and an
    /// authentication cookie was created.
    /// </response>
    /// <response code="400">
    /// The supplied request was invalid.
    /// </response>
    /// <response code="401">
    /// The authentication session or verification code was invalid.
    /// </response>
    [HttpPost("mfa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<AuthenticationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<AuthenticationProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> RespondToMultiFactorAuthenticationChallenge(
        [FromBody] RespondToMultiFactorAuthenticationChallengeCommand command,
        CancellationToken cancellationToken
    )
    {
        return (
            await _loginService.RespondToMultiFactorAuthenticationChallenge(
                command,
                cancellationToken
            )
        ).Match(HandleLoginSuccess, HandleLoginError);
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
    [ProducesResponseType<MultiFactorAuthenticationSetupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetupUser(
        [FromBody] SetupUserCommand setupUserCommand,
        CancellationToken cancellationToken
    )
    {
        SetupUserResult result = await _authorisationAdministrationService.SetupUser(
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
                    doesNotExist: () => NotFound(_setupTokenNotFound),
                    unauthorised: () => Unauthorized()
                );
            }
        );
    }

    /// <summary>
    /// Verifies the user's multi-factor authentication setup by validating the
    /// provided authentication code.
    /// </summary>
    /// <param name="command">
    /// The command containing the setup token, authentication code, and authentication
    /// session required to complete multi-factor authentication verification.
    /// </param>
    /// <param name="cancellationToken">
    /// The token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous verification operation.
    /// </returns>
    /// <response code="200">
    /// The multi-factor authentication setup was successfully verified.
    /// </response>
    /// <response code="400">
    /// The request contained invalid data or the multi-factor authentication
    /// verification could not be completed.
    /// </response>
    [HttpPost("verify-mfa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> VerifyMultiFactorAuthentication(
        [FromBody] VerifyMultiFactorAuthenticationCommand command,
        CancellationToken cancellationToken
    )
    {
        Result<VerifyMultiFactorAuthenticationError> result =
            await _authorisationAdministrationService.VerifyMultiFactorAuthentication(
                command,
                cancellationToken
            );

        return result.Match(
            Ok,
            err =>
                err.Match<ActionResult>(invalidCode: _ =>
                    BadRequest(
                        new ProblemDetails
                        {
                            Title = "Invalid multi-factor authentication code.",
                            Detail = "The supplied authentication code is invalid or has expired.",
                            Status = StatusCodes.Status400BadRequest,
                        }
                    )
                )
        );
    }

    private bool PassesCsrfValidation()
    {
        string? csrfCookie = Request.Cookies[CsrfCookieName];
        StringValues csrfHeader = Request.Headers[CsrfHeaderName];

        return !string.IsNullOrWhiteSpace(csrfCookie)
            && csrfHeader.Any(v => string.Equals(v, csrfCookie, StringComparison.Ordinal));
    }

    private ActionResult HandleLoginSuccess(AuthenticationCredentialsDto dto)
    {
        TimeSpan accessTokenLifetime = TimeSpan.FromMinutes(15);
        TimeSpan refreshTokenLifetime = TimeSpan.FromDays(14);
        Response.Cookies.Append(
            "access_token",
            dto.AccessToken,
            new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true, // HTTPS only
                SameSite = SameSiteMode.Strict,
                Expires = _dateTimeProvider.GetOffsetUtcNow() + accessTokenLifetime,
            }
        );

        var authRefreshPath = _linkGenerator.GetPathByAction(
            action: nameof(RefreshToken),
            controller: "Authentication"
        );
        Response.Cookies.Append(
            RefreshCookieName,
            dto.RefreshToken,
            new CookieOptions
            {
                Path = authRefreshPath,
                HttpOnly = true,
                Secure = true, // HTTPS only
                SameSite = SameSiteMode.Strict,
                Expires = _dateTimeProvider.GetOffsetUtcNow() + refreshTokenLifetime,
            }
        );

        var csrfToken = Guid.NewGuid().ToString("N");
        Response.Cookies.Append(
            CsrfCookieName,
            csrfToken,
            new CookieOptions
            {
                Path = authRefreshPath,
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = _dateTimeProvider.GetOffsetUtcNow() + refreshTokenLifetime,
            }
        );

        return Ok();
    }

    private ActionResult HandleLoginError(InitiateAuthenticationError error)
    {
        return error switch
        {
            InitiateAuthenticationError.Unauthorised => Unauthorized(
                AuthenticationProblemDetails.Unauthorised()
            ),
            InitiateAuthenticationError.Challenge c => Unauthorized(
                AuthenticationProblemDetails.Challenge(c.ChallengeType, c.AuthenticationSession)
            ),
            _ => throw new UnreachableException(
                $"Unhandled {nameof(InitiateAuthenticationError)} variant."
            ),
        };
    }
}
