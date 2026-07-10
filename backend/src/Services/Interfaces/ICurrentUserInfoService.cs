namespace UKPS.Api.Services.Interfaces;

public interface ICurrentUserInfoService
{
    /// <summary>
    /// Gets the information of the current user.
    /// </summary>
    /// <returns>A <see cref="CurrentUser"/> object containing the current user's information.</returns>
    CurrentUser GetCurrentUserInfo();
}
