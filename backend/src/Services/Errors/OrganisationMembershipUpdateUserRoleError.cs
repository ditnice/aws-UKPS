namespace UKPS.Api.Services.Errors;

public abstract record OrganisationMembershipUpdateUserRoleError
{
    public sealed record NotFound(int OrganisationId, int MembershipId)
        : OrganisationMembershipUpdateUserRoleError;
}
