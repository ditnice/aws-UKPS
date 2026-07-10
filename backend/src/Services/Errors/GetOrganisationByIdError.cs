namespace UKPS.Api.Services.Errors;

public abstract record GetOrganisationByIdError
{
    private protected GetOrganisationByIdError() { }

    internal sealed record NotAllowed(int OrganisationId) : GetOrganisationByIdError;

    public sealed record NotFound(int OrganisationId) : GetOrganisationByIdError;
}
