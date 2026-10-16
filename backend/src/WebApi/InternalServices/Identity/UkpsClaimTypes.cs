namespace UKPS.Api.WebApi.InternalServices.Identity;

/// <summary>
/// Defines the custom claim types used by the UKPS application.
/// </summary>
internal static class UkpsClaimTypes
{
    /// <summary>
    /// The claim type representing the identifier of the organisation associated with the user.
    /// </summary>
    public const string OrganisationId = "organisation_id";

    /// <summary>
    /// The claim type representing the role assigned to the user.
    /// </summary>
    public const string UserRole = "user_role";

    /// <summary>
    /// The claim type representing the email of the user.
    /// </summary>
    public const string Email = "email";

    /// <summary>
    /// The claim type representing the username of the user.
    /// </summary>
    public const string Username = "username";

    /// <summary>
    /// The claim type indicating that the user is authenticated but an organisation could not be
    /// resolved for them. The value is the name of the relevant <see cref="AuthenticationFailCode"/>.
    /// </summary>
    public const string OrganisationSelectionFailure = "organisation_selection_failure";

    /// <summary>
    /// The claim type representing an organisation the user can select, with one claim per
    /// organisation. The value is the organisation identifier and the user's role in it, in the
    /// format <c>{organisationId}:{userRole}</c>.
    /// </summary>
    public const string SelectableMembership = "selectable_membership";
}
