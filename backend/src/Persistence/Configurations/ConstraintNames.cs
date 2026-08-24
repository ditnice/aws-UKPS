namespace UKPS.Api.Persistence.Configurations;

internal static class ConstraintNames
{
    public static string UserMembershipRequiresOrganisation =>
        "fk_user_org_memberships_organisations_organisation_id";

    public static string UserMembershipUniqueUserAndOrgId =>
        "ix_user_org_membership_user_org_entity";
    public static string UserUniqueEmail => "ix_app_user_work_email";
}
