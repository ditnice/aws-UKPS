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
        new Claim(UkpsClaimTypes.Email, $"example.user@email.com"),
    ];

    public ICollection<Claim> Claims { get; } = DefaultClaims.ToList();
}
