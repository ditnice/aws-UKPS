using System.Globalization;
using System.Security.Claims;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.WebApi.Utilities;

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
        return new CurrentUser { OrganisationId = FindOrganisationId(), UserRole = FindUserRole() };
    }

    private int FindOrganisationId()
    {
        string? organisationIdClaim = Principal.FindFirstValue(UkpsClaimTypes.OrganisationId);
        return int.TryParse(organisationIdClaim, CultureInfo.InvariantCulture, out var orgId)
            ? orgId
            : throw new InvalidOperationException(
                $"Invalid {UkpsClaimTypes.OrganisationId} claim value."
            );
    }

    private UserRole FindUserRole()
    {
        string? userRoleClaim = Principal.FindFirstValue(UkpsClaimTypes.UserRole);
        return Enum.TryParse<UserRole>(userRoleClaim, out var role) && Enum.IsDefined(role)
            ? role
            : throw new InvalidOperationException(
                $"Invalid {UkpsClaimTypes.UserRole} claim value."
            );
    }
}
