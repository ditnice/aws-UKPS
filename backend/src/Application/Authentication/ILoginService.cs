using UKPS.Api.Application.Authentication.Dtos;
using InitiateAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;

namespace UKPS.Api.Application.Authentication;

/// <summary>
/// Defines operations for authenticating users and completing authentication
/// challenges.
/// </summary>
public interface ILoginService
{
    /// <summary>
    /// Initiates the authentication process for the specified user credentials.
    /// </summary>
    /// <param name="request">
    /// The login credentials supplied by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// The token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A result containing the authenticated user's credentials if authentication
    /// succeeds, or an error describing any additional action required or why
    /// authentication failed.
    /// </returns>
    Task<InitiateAuthenticationResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Responds to a multi-factor authentication challenge by validating the
    /// supplied authentication code and, if successful, completing the
    /// authentication process.
    /// </summary>
    /// <param name="command">
    /// The command containing the one-time authentication code and the
    /// authentication session associated with the MFA challenge.
    /// </param>
    /// <param name="cancellationToken">
    /// The token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A result containing the authenticated user's credentials if the challenge
    /// was successfully completed; otherwise, an error describing why the
    /// authentication failed.
    /// </returns>
    Task<InitiateAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        RespondToMultiFactorAuthenticationChallengeCommand command,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Refreshes the authentication tokens for an authenticated user using
    /// the supplied refresh token.
    /// </summary>
    /// <param name="command">
    /// The command containing the user's username and refresh token.
    /// </param>
    /// <param name="cancellationToken">
    /// The token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A result containing the refreshed authentication credentials if the
    /// refresh succeeds; otherwise, an error describing why the refresh
    /// operation failed.
    /// </returns>
    Task<InitiateAuthenticationResult> RefreshAuthenticationToken(
        RefreshAuthenticationTokenCommand command,
        CancellationToken cancellationToken
    );
}
