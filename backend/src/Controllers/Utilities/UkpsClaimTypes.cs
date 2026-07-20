namespace UKPS.Api.Controllers.Utilities;

/// <summary>
/// Defines the custom claim types used by the UKPS application.
/// </summary>
public static class UkpsClaimTypes
{
    /// <summary>
    /// The claim type representing the identifier of the organisation associated with the user.
    /// </summary>
    public const string OrganisationId = "organisation_id";

    /// <summary>
    /// The claim type representing the role assigned to the user.
    /// </summary>
    public const string UserRole = "user_role";
}
