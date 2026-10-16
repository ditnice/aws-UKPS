namespace UKPS.Api.Application.InternalServices.Authorisation;

/// <summary>
/// Whether the current user can perform an operation on an organisation.
/// </summary>
internal enum OrganisationAccess
{
    /// <summary>
    /// The user cannot perform the operation on the organisation.
    /// </summary>
    NotAllowed = 0,

    /// <summary>
    /// The user could perform the operation, but the organisation is not their selected one.
    /// </summary>
    NotSelected = 1,

    /// <summary>
    /// The user can perform the operation on their selected organisation.
    /// </summary>
    Allowed = 2,
}
