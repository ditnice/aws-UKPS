namespace UKPS.Api.WebApi.InternalServices.Authentication;

/// <summary>
/// Provides access to the authenticated user's access token.
/// </summary>
public interface IAccessTokenProvider
{
    /// <summary>
    /// Gets the access token for the authenticated user.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation, containing the access token.
    /// </returns>
    Task<string> GetAccessTokenAsync();
}
