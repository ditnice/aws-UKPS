using System.Security.Claims;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.WebApi.InternalServices.Authentication;

internal class DevAuthenticationClaims
{
    public static CognitoUsername DefaultUserCognitoUsername = new CognitoUsername()
    {
        Value = "14f9bc9b-ada7-4f1c-9f5c-a4e7de72d80d",
    };
    public const string DefaultUserEmail = "example.user@email.com";
    public static IReadOnlyCollection<Claim> DefaultClaims { get; } =
    [
        new Claim(UkpsClaimTypes.UserRole, UserRole.Super.ToString()),
        new Claim(UkpsClaimTypes.OrganisationId, $"{1}"),
        new Claim(UkpsClaimTypes.Email, DefaultUserEmail),
        new Claim(UkpsClaimTypes.Username, DefaultUserCognitoUsername.Value),
    ];
    public ICollection<Claim> Claims { get; } = DefaultClaims.ToList();
}
