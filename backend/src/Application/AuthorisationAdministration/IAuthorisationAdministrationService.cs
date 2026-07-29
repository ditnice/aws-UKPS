using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.AuthorisationAdministration.SetupTokenValidationError>;

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
}
