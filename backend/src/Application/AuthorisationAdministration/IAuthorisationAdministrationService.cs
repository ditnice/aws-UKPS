using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.AuthorisationAdministration.SetupTokenValidationError>;
using UserSetupResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.AuthorisationAdministration.UserSetupError>;

namespace UKPS.Api.Application.AuthorisationAdministration;

/// <summary>
/// Provides administration operations related to user authorisation and setup token validation.
/// </summary>
public interface IAuthorisationAdministrationService
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
    Task<UserSetupResult> SetupUser(SetupUserCommand command, CancellationToken cancellationToken);
}
