using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal record CreateInitialisedUserCommand
{
    public required CognitoUsername CognitoUsername { get; init; }
    public required DateTime Now { get; init; }
    public required string CurrentUserEmail { get; init; }
    public required string FullName { get; init; }
    public required string WorkEmail { get; init; }
    public required string WorkTelephone { get; init; }
    public required int OrganisationId { get; init; }
    public UserType? UserType { get; set; }
}
