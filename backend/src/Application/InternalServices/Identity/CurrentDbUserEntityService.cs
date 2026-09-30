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

    public Task<User> GetCurrentUser(CancellationToken cancellationToken) =>
        GetCurrentUser(updateQuery: null, cancellationToken);

    public async Task<User> GetCurrentUser(
        Func<IQueryable<User>, IQueryable<User>>? updateQuery,
        CancellationToken cancellationToken
    )
    {
        CognitoUsername cognitoUsername = _currentUserInfoService.GetCurrentCognitoUsername();
        IQueryable<User> query = updateQuery is not null
            ? updateQuery(_dbContext.Users)
            : _dbContext.Users;
        User? user = await query.FirstOrDefaultAsync(
            x => x.CognitoUsername == cognitoUsername,
            cancellationToken
        );
        return user
            ?? throw new InvalidOperationException(
                "Could not find the current user by Cognito username in the database as expected."
            );
    }
}
