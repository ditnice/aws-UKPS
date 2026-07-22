using Bogus;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed class DataSeederInMemory : IDataSeeder
{
    private readonly SeedDataWriter _writer;

    public DataSeederInMemory(SeedDataWriter writer)
    {
        _writer = writer;
    }

    public Task SeedData(CancellationToken cancellationToken)
    {
        Faker<SeedingDataPayload> faker = new SeedingDataPayloadFaker().UseSeed(0);
        SeedingDataPayload payload = faker.Generate();
        return _writer.Write(payload, CancellationToken.None);
    }
}
