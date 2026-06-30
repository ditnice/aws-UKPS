using UKPS.Api.DTOs;

namespace UKPS.Api.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "Returned by the public organisation service interface."
)]
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
