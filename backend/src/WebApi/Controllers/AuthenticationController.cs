using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;

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

    /// <summary>
    /// Initialises a new instance of the <see cref="AuthenticationController"/> class.
    /// </summary>
    /// <param name="authenticationService">
    /// The service responsible for handling authentication operations.
    /// </param>
    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
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
}
