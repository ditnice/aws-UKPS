global using InitiateUpdateMultiFactorAuthenticationResult = UKPS.Api.Application.Authentication.Dtos.MultiFactorAuthenticationUpdateDto;
global using VerifyMultiFactorAuthenticationUpdateResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.VerifyMultiFactorAuthenticationUpdateError>;
using UKPS.Api.Application.Authentication.Dtos;
using ResendSetupTokenResult = UKPS.Api.Application.Common.Result<
    System.Guid,
    UKPS.Api.Application.Authentication.Errors.ResendSetupTokenError
>;
using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.SetupTokenValidationError>;
using SetupUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.MultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.Authentication.Errors.UserSetupError
>;
using VerifyMultiFactorAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.VerifyMultiFactorAuthenticationError
>;

namespace UKPS.Api.Application.Authentication;

/// <summary>
/// Provides administration operations related to user authorisation and setup token validation.
/// </summary>
public interface IIdentityAdministrationService
{
    /// <summary>
    /// Validates whether a setup token is valid and can be used for authorisation.
    /// </summary>
    /// <param name="setupToken">The setup token to validate.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A result indicating whether the setup token is valid or the reason validation failed.
    /// </returns>
    Task<SetupTokenValidationResult> Validate(Guid setupToken, CancellationToken cancellationToken);

    /// <summary>
    /// Reissues an expired setup token and emails a new setup link to the user's
    /// registered email address.
    /// </summary>
    /// <param name="command">
    /// The command identifying the record to reissue, by either its expired setup
    /// token or a correlation id returned from a previous resend.
    /// </param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A result containing the new correlation id if a new setup link was sent, or
    /// the reason it could not be.
    /// </returns>
    Task<ResendSetupTokenResult> ResendSetupToken(
        ResendSetupTokenCommand command,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Completes the user setup process by validating the setup command and creating
    /// the user's credentials.
    /// </summary>
    /// <param name="command">The command containing the setup token and new password details.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A result indicating whether the user setup completed successfully or the reason it failed.
    /// </returns>
    Task<SetupUserResult> SetupUser(SetupUserCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Initiates the process of updating the multi-factor authentication (MFA)
    /// configuration for the user identified by the supplied access token.
    /// </summary>
    /// <param name="accessToken">
    /// A valid access token identifying the authenticated user whose MFA
    /// configuration is to be updated.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is an
    /// <see cref="InitiateUpdateMultiFactorAuthenticationResult"/> containing the
    /// outcome of the initiation, including any information needed to complete the
    /// MFA update.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="accessToken"/> is <c>null</c>, empty, or whitespace.
    /// </exception>
    Task<InitiateUpdateMultiFactorAuthenticationResult> InitiateUpdateMultiFactorAuthentication(
        string accessToken,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Verifies the user's multi-factor authentication setup by validating the
    /// provided authentication code and completing the MFA configuration process.
    /// </summary>
    /// <param name="command">
    /// The command containing the setup token, authentication code, and authentication
    /// session required to verify the MFA configuration.
    /// </param>
    /// <param name="cancellationToken">
    /// The token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous verification operation.
    /// </returns>
    Task<VerifyMultiFactorAuthenticationResult> VerifyMultiFactorAuthentication(
        VerifyMultiFactorAuthenticationCommand command,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Verifies a pending multi-factor authentication (MFA) update for the user identified by the supplied access token.
    /// </summary>
    /// <param name="accessToken">
    /// The access token identifying and authorizing the user whose MFA update is being verified.
    /// </param>
    /// <param name="command">
    /// The command containing the details required to verify the MFA update, such as the verification code.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a
    /// <see cref="VerifyMultiFactorAuthenticationUpdateResult"/> describing the outcome of the verification.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
    /// </exception>
    Task<VerifyMultiFactorAuthenticationUpdateResult> VerifyMultiFactorAuthenticationUpdate(
        string accessToken,
        VerifyMultiFactorAuthenticationUpdateCommand command,
        CancellationToken cancellationToken
    );
}
