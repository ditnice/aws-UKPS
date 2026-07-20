using Microsoft.EntityFrameworkCore;

namespace UKPS.Api.Data.Seeding;

internal sealed class SeedDataWriter
{
    private readonly AppDbContext _appDbContext;

    public SeedDataWriter(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task Write(SeedingDataPayload seedingData, CancellationToken cancellationToken)
    {
        await ClearExistingDatabase(cancellationToken);

        foreach (var entity in seedingData.GetAllEntities())
        {
            await _appDbContext.AddAsync(entity, cancellationToken);
        }
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ClearExistingDatabase(CancellationToken cancellationToken)
    {
        await _appDbContext.Database.EnsureDeletedAsync(cancellationToken);
        await _appDbContext.Database.MigrateAsync(cancellationToken);
    }
}
