using System.Security.Claims;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.Tests.Utilities.Fixtures;

public class TestAuthenticationOptions
{
    public static IReadOnlyCollection<Claim> DefaultClaims { get; } =
    [
        new Claim(UkpsClaimTypes.UserRole, UserRole.Super.ToString()),
        new Claim(UkpsClaimTypes.OrganisationId, $"{1}"),
        new Claim(UkpsClaimTypes.Email, "example.user@email.com"),
        new Claim(UkpsClaimTypes.Username, "24a97ae0-d7d2-4c6b-88de-d835b13e8038"),
    ];

    public ICollection<Claim> Claims { get; } = DefaultClaims.ToList();
}
