namespace UKPS.Api.Services.Errors;

public abstract record GetOrganisationByIdError
{
    private protected GetOrganisationByIdError() { }

    public sealed record NotFound(int OrganisationId) : GetOrganisationByIdError;
}
