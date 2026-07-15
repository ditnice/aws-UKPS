namespace UKPS.Api.Services.Interfaces;

/// <summary>
/// A service for retrieving information about the current user of the system.
/// </summary>
public interface ICurrentUserInfoService
{
    /// <summary>
    /// Gets the information of the current user.
    /// </summary>
    /// <returns>A <see cref="CurrentUser"/> object containing the current user's information.</returns>
    CurrentUser GetCurrentUserInfo();
}
