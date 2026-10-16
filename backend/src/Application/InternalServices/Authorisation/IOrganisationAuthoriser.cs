namespace UKPS.Api.Application.InternalServices.Authorisation;

internal interface IOrganisationAuthoriser
{
    ValueOrAll<int> GetAuthorisedOrganisations(Operation operation);

    /// <summary>
    /// Gets whether the current user can perform the operation on the organisation. Use this
    /// instead of <see cref="CanPerformOperationOnOrganisation"/> to look up the requested
    /// resource before offering to switch organisation.
    /// </summary>
    OrganisationAccess GetOrganisationAccess(Operation operation, int organisationId);

    /// <summary>
    /// Checks whether the current user can perform the operation on the organisation.
    /// </summary>
    /// <exception cref="SelectedOrganisationMismatchException">
    /// The access is <see cref="OrganisationAccess.NotSelected"/>.
    /// </exception>
    bool CanPerformOperationOnOrganisation(Operation operation, int organisationId);
}
