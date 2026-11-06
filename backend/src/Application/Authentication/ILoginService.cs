global using ConfirmNewPasswordResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.InternalServices.Identity.ConfirmNewPasswordError>;
global using InitiateForgottenPasswordProcedureResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.InitiateForgottenPasswordProcedureError>;
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

    /// <summary>
    /// Ends the current authentication session by revoking the supplied refresh token.
    /// </summary>
    /// <param name="refreshToken">
    /// The refresh token for the session that should be revoked.
    /// </param>
    /// <param name="cancellationToken">
    /// The token used to cancel the operation.
    /// </param>
    Task SignOut(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Initiates the forgotten password flow for the user associated with the
    /// specified email address, typically by sending a password reset code or
    /// link to that address.
    /// </summary>
    /// <param name="command">
    /// The command containing the email address of the user who has forgotten
    /// their password.
    /// </param>
    /// <param name="cancellationToken">
    /// The token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A result indicating whether the forgotten password flow was successfully
    /// initiated, or an error describing why it could not be started, such as
    /// the number of permitted attempts having been exceeded.
    /// </returns>
    Task<InitiateForgottenPasswordProcedureResult> InitiateForgottenPasswordProcedure(
        InitiateForgottenPasswordProcedureCommand command,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Confirms a password reset by validating the confirmation code and setting a new password.
    /// </summary>
    /// <param name="command">
    /// The command containing the confirmation code, the new password, and the details identifying the user.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ConfirmNewPasswordResult"/> that is either a success, or one of the following errors:
    /// <list type="bullet">
    /// <item><description>The confirmation code did not match or has expired.</description></item>
    /// <item><description>The new password does not meet the password requirements.</description></item>
    /// <item><description>Too many requests have been made.</description></item>
    /// <item><description>Too many failed attempts have been made.</description></item>
    /// </list>
    /// </returns>
    Task<ConfirmNewPasswordResult> ConfirmNewPassword(
        ConfirmNewPasswordCommand command,
        CancellationToken cancellationToken
    );
}
