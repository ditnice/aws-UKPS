namespace UKPS.Api.Services.Errors;

public abstract record UpdateOrganisationDetailsError
{
    public sealed record NotAllowed(int OrganisationId) : UpdateOrganisationDetailsError;

    public sealed record NotFound(int OrganisationId) : UpdateOrganisationDetailsError;
}
