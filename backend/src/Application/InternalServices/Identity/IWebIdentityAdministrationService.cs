namespace UKPS.Api.Application.InternalServices.Identity;

/// <summary>
/// Provides an abstraction for administering users within a web identity provider.
/// </summary>
/// <remarks>
/// This interface encapsulates operations for managing user identities without
/// exposing the underlying identity provider implementation.
/// </remarks>
public interface IWebIdentityAdministrationService
{
    /// <summary>
    /// Creates a new user account using the specified email address.
    /// </summary>
    /// <param name="email">
    /// The email address associated with the new user account.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous user creation operation.
    /// </returns>
    Task CreateNewUser(string email, CancellationToken cancellationToken);
}
