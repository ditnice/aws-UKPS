using System.Security.Claims;

namespace UKPS.Api.Tests.Fixtures;

public class TestAuthenticationOptions
{
    public static IReadOnlyCollection<Claim> DefaultClaims { get; } =
    [new Claim("ExampleClaimType", "ExampleClaimValue")];

    public ICollection<Claim> Claims { get; } = DefaultClaims.ToList();
}
