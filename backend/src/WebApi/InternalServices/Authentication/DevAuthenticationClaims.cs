using System.Security.Claims;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.WebApi.InternalServices.Authentication;

internal class DevAuthenticationClaims
{
    public const string DefaultUserEmail = "example.user@email.com";
    public static IReadOnlyCollection<Claim> DefaultClaims { get; } =
    [
        new Claim(UkpsClaimTypes.UserRole, UserRole.Super.ToString()),
        new Claim(UkpsClaimTypes.OrganisationId, $"{1}"),
        new Claim(UkpsClaimTypes.Email, DefaultUserEmail),
    ];

    public ICollection<Claim> Claims { get; } = DefaultClaims.ToList();
}
