using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.InternalServices.Identity;

/// <summary>
/// A mock implementation of the <see cref="ICurrentUserInfoService"/> interface.
/// This will be replaced with a real implementation once the authentication
/// system is in place. For now, it returns a hardcoded user with a Super role
/// and an OrganisationId of 1.
/// </summary>
internal sealed class MockCurrentUserInfoService : ICurrentUserInfoService
{
    private readonly AppDbContext _appDbContext;

    public MockCurrentUserInfoService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public CurrentUser GetCurrentUserInfo()
    {
        var sampleUser =
            _appDbContext
                .UserOrgMemberships.Where(x => x.UserRole == UserRole.Super)
                .Where(x => x.OrganisationId == 1)
                .FirstOrDefault(x => x.IsAuthorised())
            ?? throw new InvalidOperationException(
                "A super user was not seeded for organisation 1."
            );

        return new CurrentUser
        {
            OrganisationId = 1,
            UserRole = UserRole.Super,
            Email = sampleUser.User!.WorkEmail,
        };
    }
}
