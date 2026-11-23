using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.WebApi.InternalServices.Authentication;

/// <summary>
/// A user that development authentication can act as. Seeding creates one per role, as an active
/// member of <paramref name="OrganisationId"/>. Seeded organisation ids follow the seeding order,
/// so organisation <c>n</c> is the <c>n</c>th seeded organisation.
/// </summary>
internal sealed record DevAuthenticationUser(
    CognitoUsername CognitoUsername,
    string Email,
    int OrganisationId
);
