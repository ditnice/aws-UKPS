namespace UKPS.Api.Services.Errors;

public abstract record OrganisationMembershipUpdateUserRoleError
{
    internal sealed record NotAllowed(int OrganisationId)
        : OrganisationMembershipUpdateUserRoleError;

    public sealed record NotFound(int OrganisationId, int MembershipId)
        : OrganisationMembershipUpdateUserRoleError;
}
