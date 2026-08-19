namespace UKPS.Api.Persistence.Configurations;

internal static class ConstraintNames
{
    public static string UserMembershipRequiresOrganisation =>
        "fk_user_org_memberships_organisations_organisation_id";
}
