namespace UKPS.Api.Application.InternalServices.Authorisation;

internal static class OrganisationAccessExtensions
{
    /// <summary>
    /// Throws when the organisation is accessible but not the user's selected organisation.
    /// </summary>
    /// <exception cref="SelectedOrganisationMismatchException">
    /// The access is <see cref="OrganisationAccess.NotSelected"/>.
    /// </exception>
    public static void ThrowIfNotSelected(this OrganisationAccess access, int organisationId)
    {
        if (access == OrganisationAccess.NotSelected)
        {
            throw new SelectedOrganisationMismatchException(organisationId);
        }
    }
}
