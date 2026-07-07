namespace UKPS.Api.Services.Errors;

public abstract record GetUsersError
{
    private protected GetUsersError() { }

    public sealed record OrganisationNotFound(int OrganisationId) : GetUsersError;
}
