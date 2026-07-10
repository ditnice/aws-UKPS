namespace UKPS.Api.Services.Errors;

public abstract record OrganisationMembershipDeactivateUserError
{
    internal sealed record NotAllowed(int OrganisationId)
        : OrganisationMembershipDeactivateUserError;

    public sealed record NotFound() : OrganisationMembershipDeactivateUserError;
}
