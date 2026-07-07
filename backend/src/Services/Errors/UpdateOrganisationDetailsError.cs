namespace UKPS.Api.Services.Errors;

public abstract record UpdateOrganisationDetailsError
{
    private protected UpdateOrganisationDetailsError() { }

    public sealed record NotFound(int OrganisationId) : UpdateOrganisationDetailsError;
}
