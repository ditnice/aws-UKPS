namespace UKPS.Api.Persistence.Data.Seeding;

internal interface IDataSeeder
{
    Task SeedData(CancellationToken cancellationToken);
}
