using UKPS.Api.Application.Authentication.Dtos;
using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.SetupTokenValidationError>;
using SetupUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.MultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.Authentication.Errors.UserSetupError
>;
using VerifyMultiFactorAuthenticationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.VerifyMultiFactorAuthenticationError>;

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
}
