using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Application.InternalServices.Identity;

internal sealed class CurrentDbUserEntityService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserInfoService _currentUserInfoService;

    public CurrentDbUserEntityService(
        AppDbContext dbContext,
        ICurrentUserInfoService currentUserInfoService
    )
    {
        _dbContext = dbContext;
        _currentUserInfoService = currentUserInfoService;
    }

    public async Task<User> GetCurrentUser(
        CancellationToken cancellationToken,
        Func<IQueryable<User>, IQueryable<User>>? updateQuery = null
    )
    {
        CurrentUser currentUser = _currentUserInfoService.GetCurrentUserInfo();
        IQueryable<User> query = updateQuery is not null
            ? updateQuery(_dbContext.Users)
            : _dbContext.Users;
        User? user = await query.FirstOrDefaultAsync(
            x => x.CognitoUsername == currentUser.CognitoUsername,
            cancellationToken
        );
        return user
            ?? throw new InvalidOperationException(
                "Could not find current user by email in the database as expected."
            );
    }
}
