using System.Globalization;
using System.Security.Claims;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.WebApi.InternalServices.Identity;

internal class WebApiCurrentUserInfoService : ICurrentUserInfoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private ClaimsPrincipal Principal =>
        _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public WebApiCurrentUserInfoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser GetCurrentUserInfo()
    {
        return ParseFromUserPrincipal(Principal);
    }

    private static string FindUserEmail(ClaimsPrincipal claimsPrincipal)
    {
        string? userEmailClaim = claimsPrincipal.FindFirstValue(UkpsClaimTypes.Email);
        return userEmailClaim
            ?? throw new InvalidOperationException($"Invalid {UkpsClaimTypes.Email} claim value.");
    }

    private static int FindOrganisationId(ClaimsPrincipal claimsPrincipal)
    {
        string? organisationIdClaim = claimsPrincipal.FindFirstValue(UkpsClaimTypes.OrganisationId);
        return int.TryParse(organisationIdClaim, CultureInfo.InvariantCulture, out var orgId)
            ? orgId
            : throw new InvalidOperationException(
                $"Invalid {UkpsClaimTypes.OrganisationId} claim value."
            );
    }

    private static UserRole FindUserRole(ClaimsPrincipal claimsPrincipal)
    {
        string? userRoleClaim = claimsPrincipal.FindFirstValue(UkpsClaimTypes.UserRole);
        return Enum.TryParse<UserRole>(userRoleClaim, out var role) && Enum.IsDefined(role)
            ? role
            : throw new InvalidOperationException(
                $"Invalid {UkpsClaimTypes.UserRole} claim value."
            );
    }

    public static CurrentUser ParseFromUserPrincipal(ClaimsPrincipal claimsPrincipal)
    {
        return new CurrentUser
        {
            OrganisationId = FindOrganisationId(claimsPrincipal),
            UserRole = FindUserRole(claimsPrincipal),
            Email = FindUserEmail(claimsPrincipal),
        };
    }
}
