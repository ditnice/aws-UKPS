using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

public record UserOrganisationMembershipDto
{
    public required int UserId { get; init; }
    public required int OrganisationId { get; init; }
    public required UserRole UserRole { get; init; }
    public required UserOrgStatus Status { get; init; }
    public required PharmaceuticalEntity AllowedPharmaceuticalEntity { get; init; }
    public required DateTime? CreatedAt { get; init; }
}
