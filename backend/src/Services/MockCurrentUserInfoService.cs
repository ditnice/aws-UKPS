using UKPS.Api.Enums;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Services;

/// <summary>
/// A mock implementation of the <see cref="ICurrentUserInfoService"/> interface.
/// This will be replaced with a real implementation once the authentication
/// system is in place. For now, it returns a hardcoded user with a Super role
/// and an OrganisationId of 1.
/// </summary>
internal sealed class MockCurrentUserInfoService : ICurrentUserInfoService
{
    public CurrentUserInfo GetCurrentUserInfo()
    {
        return new CurrentUserInfo { OrganisationId = 1, UserRole = UserRole.Super };
    }
}
