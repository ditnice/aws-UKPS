namespace UKPS.Api.Services.Results;

public enum DeactivateOrganisationUserStatus
{
    Success = 0,
    OrganisationNotFound = 1,
    UserNotFound = 2,
    AlreadyInactive = 3,
}
