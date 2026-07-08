using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Tests.Fixtures;

internal static class EntityFactory
{
    public static Organisation CreateOrganisation(int id) =>
        new()
        {
            Id = id,
            OrganisationName = "Gov Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            HeadOfficeAddress = "1 High Street\nLondon\nEC1A 1AA",
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
        };

    public static User CreateUser(int id, string workEmail) =>
        new()
        {
            Id = id,
            Username = workEmail,
            FirstName = "Test",
            LastName = "User",
            WorkEmail = workEmail,
        };

    public static UserOrgMembership CreateMembership(
        int id,
        int userId,
        int organisationId,
        UserRole role = UserRole.Standard,
        UserOrgStatus status = UserOrgStatus.Active,
        PharmaceuticalEntity allowedPharmaceuticalEntity = default
    ) =>
        new()
        {
            Id = id,
            UserId = userId,
            OrganisationId = organisationId,
            UserRole = role,
            Status = status,
            AllowedPharmaceuticalEntity = allowedPharmaceuticalEntity,
            // Use UTC because PostgreSQL timestamptz columns reject unspecified DateTime values.
            CreatedAt = DateTime.UtcNow,
        };
}
