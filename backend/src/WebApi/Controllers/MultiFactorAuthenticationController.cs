using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints that allow an authenticated user to update their
/// multi-factor authentication (MFA) settings, including initiating the update
/// and verifying the resulting authentication code.
/// </summary>
/// <remarks>
/// All actions on this controller require the caller to be authenticated.
/// </remarks>
[ApiController]
[Route("mfa")]
[Authorize]
public class MultiFactorAuthenticationController : ControllerBase
{
    private readonly IIdentityAdministrationService _authorisationAdministrationService;
    private readonly IAccessTokenProvider _accessTokenProvider;

    private readonly ProblemDetails _invalidMfaSetupCode = new ProblemDetails
    {
        Title = "Invalid multi-factor authentication code.",
        Detail = "The supplied authentication code is invalid or has expired.",
        Status = StatusCodes.Status400BadRequest,
    };

    /// <summary>
    /// Initialises a new instance of the <see cref="MultiFactorAuthenticationController"/> class.
    /// </summary>
    /// <param name="authorisationAdministrationService">
    /// The service responsible for managing authorisation administration operations,
    /// such as user onboarding, setup token validation, and password management.
    /// </param>
    /// <param name="accessTokenProvider">
    /// The provider responsible for generating and issuing access tokens
    /// for authenticated users.
    /// </param>
    public MultiFactorAuthenticationController(
        IIdentityAdministrationService authorisationAdministrationService,
        IAccessTokenProvider accessTokenProvider
    )
    {
        _authorisationAdministrationService = authorisationAdministrationService;
        _accessTokenProvider = accessTokenProvider;
    }

    /// <summary>
    /// Initiates the process of updating the authenticated user's multi-factor authentication settings.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing a <see cref="MultiFactorAuthenticationSetupDto"/>
    /// with the details required to configure multi-factor authentication.
    /// </returns>
    [HttpPost("update", Name = nameof(InitiateUpdateMultiFactorAuthentication))]
    [ProducesResponseType<MultiFactorAuthenticationSetupDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult> InitiateUpdateMultiFactorAuthentication(
        CancellationToken cancellationToken
    )
    {
        string accessToken = await _accessTokenProvider.GetAccessTokenAsync();
        InitiateUpdateMultiFactorAuthenticationResult result =
            await _authorisationAdministrationService.InitiateUpdateMultiFactorAuthentication(
                accessToken,
                cancellationToken
            );

        return Ok(result);
    }

    /// <summary>
    /// Verifies the multi-factor authentication code provided by the authenticated user
    /// to complete the update of their multi-factor authentication settings.
    /// </summary>
    /// <param name="command">
    /// The command containing the multi-factor authentication verification code.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> indicating whether the multi-factor authentication
    /// update was successful. Returns <see cref="StatusCodes.Status400BadRequest"/>
    /// if the verification code is invalid, or <see cref="StatusCodes.Status404NotFound"/>
    /// if the requested resource cannot be found.
    /// </returns>
    [HttpPost("verify", Name = nameof(VerifyMultiFactorAuthenticationUpdate))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> VerifyMultiFactorAuthenticationUpdate(
        [FromBody] VerifyMultiFactorAuthenticationUpdateCommand command,
        CancellationToken cancellationToken
    )
    {
        string accessToken = await _accessTokenProvider.GetAccessTokenAsync();
        VerifyMultiFactorAuthenticationUpdateResult result =
            await _authorisationAdministrationService.VerifyMultiFactorAuthenticationUpdate(
                accessToken,
                command,
                cancellationToken
            );

        return result.Match(
            Ok,
            err => err.Match<ActionResult>(invalidCode: _ => BadRequest(_invalidMfaSetupCode))
        );
    }
}
