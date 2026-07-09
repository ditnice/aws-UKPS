namespace UKPS.Api.Services.Interfaces;

internal interface IOrganisationAuthoriser
{
    ValueOrAll<int> GetAuthorisedOrganisations(Operation operation);
    bool CanPerformOperationOnOrganisation(Operation operation, int organisationId) =>
        GetAuthorisedOrganisations(operation).Contains(organisationId);
}
