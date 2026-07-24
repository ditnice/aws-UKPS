using UKPS.Api.Application.Authentication.Dtos;
using LoginResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.LoginError
>;

namespace UKPS.Api.Application.Authentication;

/// <summary>
/// Defines operations for authenticating users.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user using the supplied login credentials.
    /// </summary>
    /// <param name="loginRequest">
    /// The username and password provided by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the authentication request.
    /// </param>
    /// <returns>
    /// A result containing either the authentication credentials or an authentication error.
    /// </returns>
    Task<LoginResult> Login(LoginRequest loginRequest, CancellationToken cancellationToken);
}
