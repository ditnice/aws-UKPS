using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Application.InternalServices.Identity;

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

    /// <summary>
    /// Gets the Amazon Cognito username of the current user. Unlike
    /// <see cref="GetCurrentUserInfo"/>, this does not require an organisation to have
    /// been resolved for the user.
    /// </summary>
    /// <returns>The Cognito username of the current user.</returns>
    CognitoUsername GetCurrentCognitoUsername();
}
