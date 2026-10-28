using UKPS.Api.Application.InternalServices.Authorisation;

namespace UKPS.Api.Application.InternalServices.Identity;

internal static class CurrentUserInfoServiceExtensions
{
    public static bool IsCurrentUser(this ICurrentUserInfoService userInfoService, string email)
    {
        CurrentUser currentUser = userInfoService.GetCurrentUserInfo();
        return string.Equals(currentUser.Email, email, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Throws when the current user is a member of the organisation but has not selected it.
    /// Call this before authorising, so the user is offered the switch regardless of their role
    /// in the organisation.
    /// </summary>
    /// <exception cref="SelectedOrganisationMismatchException">
    /// The organisation is selectable but is not the current user's selected organisation.
    /// </exception>
    public static void ThrowIfOrganisationNotSelected(
        this ICurrentUserInfoService userInfoService,
        int organisationId
    )
    {
        CurrentUser currentUser = userInfoService.GetCurrentUserInfo();
        if (
            currentUser.OrganisationId != organisationId
            && currentUser.SelectableOrganisationIds.Contains(organisationId)
        )
        {
            throw new SelectedOrganisationMismatchException(organisationId);
        }
    }
}
