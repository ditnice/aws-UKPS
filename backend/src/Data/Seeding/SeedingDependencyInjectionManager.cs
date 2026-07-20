using Microsoft.EntityFrameworkCore;

namespace UKPS.Api.Data.Seeding;

internal static class SeedingDependencyInjectionManager
{
    public static IServiceCollection AddSeedingServices(this IServiceCollection collection)
    {
        collection.AddTransient<SeedDataWriter>();
        collection.AddTransient<IDataSeeder, DataSeederInMemory>();
        return collection;
    }
}
