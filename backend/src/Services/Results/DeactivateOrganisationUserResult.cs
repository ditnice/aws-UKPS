using UKPS.Api.DTOs;

namespace UKPS.Api.Services.Results;

public sealed class DeactivateOrganisationUserResult
{
    public DeactivateOrganisationUserStatus Status { get; init; }
    public UserListItemDto? User { get; init; }

    public static DeactivateOrganisationUserResult Success(UserListItemDto user) =>
        new() { Status = DeactivateOrganisationUserStatus.Success, User = user };

    public static DeactivateOrganisationUserResult OrganisationNotFound() =>
        new() { Status = DeactivateOrganisationUserStatus.OrganisationNotFound };

    public static DeactivateOrganisationUserResult UserNotFound() =>
        new() { Status = DeactivateOrganisationUserStatus.UserNotFound };

    public static DeactivateOrganisationUserResult AlreadyInactive() =>
        new() { Status = DeactivateOrganisationUserStatus.AlreadyInactive };
}
