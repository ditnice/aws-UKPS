namespace UKPS.Api.Application.InternalServices.Identity;

internal static class CurrentUserInfoServiceExtensions
{
    public static bool IsCurrentUser(this ICurrentUserInfoService userInfoService, string email)
    {
        CurrentUser currentUser = userInfoService.GetCurrentUserInfo();
        return string.Equals(currentUser.Email, email, StringComparison.OrdinalIgnoreCase);
    }
}
