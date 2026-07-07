using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

public record OrganisationMembershipDto
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required int OrganisationId { get; init; }
    public required UserRole UserRole { get; init; }
    public required UserOrgStatus Status { get; init; }
    public required PharmaceuticalEntity AllowedPharmaceuticalEntity { get; init; }
    public required DateTime CreatedAt { get; init; }
}
