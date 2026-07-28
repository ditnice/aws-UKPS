using Microsoft.EntityFrameworkCore;

namespace UKPS.Api.Persistence;

internal sealed class DatabaseMigrator(AppDbContext dbContext) : IDatabaseMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
