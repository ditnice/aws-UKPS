using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

public sealed record CreateUserRequestDto
{
    public required string Username { get; init; }
    public required UserType UserType { get; init; }
    public required string Title { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string JobTitle { get; init; }
    public required string WorkTelephone { get; init; }
    public required string WorkEmail { get; init; }
    public required int OrganisationId { get; init; }
}
