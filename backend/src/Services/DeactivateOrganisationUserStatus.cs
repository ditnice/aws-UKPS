namespace UKPS.Api.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "Returned by the public organisation service result."
)]
public enum DeactivateOrganisationUserStatus
{
    Success = 0,
    OrganisationNotFound = 1,
    UserNotFound = 2,
    AlreadyInactive = 3,
}
