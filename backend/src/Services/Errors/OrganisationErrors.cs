using UKPS.Api.Services.Results;

namespace UKPS.Api.Services.Errors;

public static class OrganisationErrors
{
    public static ResultError OrganisationNotFound { get; } =
        new("Organisation.NotFound", ErrorType.NotFound, "Organisation not found.");

    public static ResultError UserNotFoundInOrganisation { get; } =
        new("Organisation.UserNotFound", ErrorType.NotFound, "User not found in organisation.");

    public static ResultError UserAlreadyInactive { get; } =
        new("Organisation.UserAlreadyInactive", ErrorType.Validation, "User is already inactive.");
}
