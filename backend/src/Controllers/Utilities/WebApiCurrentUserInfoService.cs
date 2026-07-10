using System.Globalization;
using System.Security.Claims;
using UKPS.Api.Enums;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Controllers.Utilities;

public class WebApiCurrentUserInfoService : ICurrentUserInfoService
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
        var organisationIdClaim = Principal.FindFirstValue(UkpsClaimTypes.OrganisationId);
        var organisationId = int.TryParse(
            organisationIdClaim,
            CultureInfo.InvariantCulture,
            out var orgId
        )
            ? orgId
            : throw new InvalidOperationException("Invalid organisation_id claim value.");
        var userRoleClaim = Principal.FindFirstValue(UkpsClaimTypes.UserRole);
        var userRole = Enum.TryParse<UserRole>(userRoleClaim, out var role)
            ? role
            : throw new InvalidOperationException("Invalid user_role claim value.");

        return new CurrentUser { OrganisationId = organisationId, UserRole = userRole };
    }
}
