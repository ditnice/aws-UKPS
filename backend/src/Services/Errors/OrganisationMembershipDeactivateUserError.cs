namespace UKPS.Api.Services.Errors;

public abstract record OrganisationMembershipDeactivateUserError
{
    public sealed record NotFound() : OrganisationMembershipDeactivateUserError;
}
