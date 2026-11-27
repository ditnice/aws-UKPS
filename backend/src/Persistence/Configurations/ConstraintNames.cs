namespace UKPS.Api.Persistence.Configurations;

internal static class ConstraintNames
{
    public static string UserMembershipRequiresOrganisation =>
        "fk_user_org_memberships_organisations_organisation_id";

    public static string UserMembershipUniqueUserAndOrgId =>
        "ix_user_org_membership_user_org_entity";
    public static string UserUniqueNormalisedEmail => "ix_app_user_normalised_work_email";
    public static string UsersCannotHaveMultipleSelectedCurrentOrganisations =>
        "ux_user_org_membership_current_org_per_user";
    public static string MembershipRequestsShouldNotBeApprovedAndRejected =>
        "ck_membership_request_approved_at_rejected_at";

    public static string WorkEmailNoWhitespace => "work_email_no_whitespace";

    public static string NameAndIdentifierUniqueDisplayOrderPerList =>
        "ix_record_name_and_identifier_product_detail_type_display_order";
}
