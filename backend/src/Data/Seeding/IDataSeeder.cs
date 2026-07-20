namespace UKPS.Api.Data.Seeding;

internal interface IDataSeeder
{
    Task SeedData(CancellationToken cancellationToken);
}
