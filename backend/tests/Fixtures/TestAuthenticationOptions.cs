using System.Security.Claims;
using UKPS.Api.Enums;
using UKPS.Api.WebApi.Controllers.Utilities;

namespace UKPS.Api.Tests.Fixtures;

public class TestAuthenticationOptions
{
    public static IReadOnlyCollection<Claim> DefaultClaims { get; } =
    [
        new Claim(UkpsClaimTypes.UserRole, UserRole.Super.ToString()),
        new Claim(UkpsClaimTypes.OrganisationId, $"{1}"),
    ];

    public ICollection<Claim> Claims { get; } = DefaultClaims.ToList();
}
