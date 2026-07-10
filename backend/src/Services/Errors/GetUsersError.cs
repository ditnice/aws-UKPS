namespace UKPS.Api.Services.Errors;

public abstract record GetUsersError
{
    internal sealed record NotAllowed(int OrganisationId) : GetUsersError;

    public sealed record OrganisationNotFound(int OrganisationId) : GetUsersError;
}
