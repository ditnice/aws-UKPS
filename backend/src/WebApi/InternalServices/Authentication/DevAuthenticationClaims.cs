using System.Globalization;
using System.Security.Claims;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.WebApi.InternalServices.Authentication;

internal class DevAuthenticationClaims
{
    /// <summary>
    /// A request with this cookie set to a <see cref="UserRole"/> acts as that role's
    /// <see cref="RoleUsers"/> user, so E2E tests can switch role without restarting the API.
    /// </summary>
    public const string RoleCookieName = "ukps_dev_role";

    public static CognitoUsername DefaultUserCognitoUsername = new CognitoUsername()
    {
        Value = "14f9bc9b-ada7-4f1c-9f5c-a4e7de72d80d",
    };
    public const string DefaultUserEmail = "example.user@email.com";

    /// <summary>The internal organisation the Super user belongs to.</summary>
    public const int DefaultOrganisationId = 1;

    /// <summary>
    /// The pharma company organisation the Champion and Standard users belong to. They're kept out
    /// of the Super user's organisation so E2E tests acting as different roles change different
    /// data.
    /// </summary>
    public const int RoleOrganisationId = 2;

    /// <summary>
    /// The seeded user each role acts as. The frontend E2E tests rely on these emails
    /// (frontend/tests/e2e/helpers/roles.ts).
    /// </summary>
    public static IReadOnlyDictionary<UserRole, DevAuthenticationUser> RoleUsers { get; } =
        new Dictionary<UserRole, DevAuthenticationUser>
        {
            [UserRole.Super] = new(
                DefaultUserCognitoUsername,
                DefaultUserEmail,
                DefaultOrganisationId
            ),
            [UserRole.Champion] = new(
                new CognitoUsername() { Value = "5b0c7a3e-2f4d-4c1a-9e8b-6d2f1a0c3b71" },
                "dev.champion@email.com",
                RoleOrganisationId
            ),
            [UserRole.Standard] = new(
                new CognitoUsername() { Value = "8e4d2b6f-7a1c-4f3e-b5d9-0c6a2e8f1d42" },
                "dev.standard@email.com",
                RoleOrganisationId
            ),
        };

    public static IReadOnlyCollection<Claim> DefaultClaims =>
        [
            new Claim(UkpsClaimTypes.UserRole, UserRole.Super.ToString()),
            new Claim(UkpsClaimTypes.OrganisationId, $"{DefaultOrganisationId}"),
            new Claim(UkpsClaimTypes.Email, DefaultUserEmail),
            new Claim(UkpsClaimTypes.Username, DefaultUserCognitoUsername.Value),
            new Claim("auth_time", $"{DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds()}"),
        ];

    public ICollection<Claim> Claims { get; } = DefaultClaims.ToList();

    internal DevAuthenticationClaims UpdateClaim(Claim claim)
    {
        var existingClaim = Claims.FirstOrDefault(existingClaim =>
            string.Equals(existingClaim.Type, claim.Type, StringComparison.Ordinal)
        );

        if (existingClaim is not null)
        {
            Claims.Remove(existingClaim);
        }

        Claims.Add(claim);
        return this;
    }

    /// <summary>
    /// These claims, but acting as the role's seeded user. The configured claims are unchanged.
    /// </summary>
    internal IReadOnlyCollection<Claim> ForRole(UserRole role)
    {
        DevAuthenticationUser user = RoleUsers[role];
        string[] replacedTypes =
        [
            UkpsClaimTypes.UserRole,
            UkpsClaimTypes.Email,
            UkpsClaimTypes.Username,
            UkpsClaimTypes.OrganisationId,
        ];

        return
        [
            .. Claims.Where(claim => !replacedTypes.Contains(claim.Type, StringComparer.Ordinal)),
            new Claim(UkpsClaimTypes.UserRole, role.ToString()),
            new Claim(UkpsClaimTypes.Email, user.Email),
            new Claim(UkpsClaimTypes.Username, user.CognitoUsername.Value),
            new Claim(
                UkpsClaimTypes.OrganisationId,
                user.OrganisationId.ToString(CultureInfo.InvariantCulture)
            ),
        ];
    }
}
