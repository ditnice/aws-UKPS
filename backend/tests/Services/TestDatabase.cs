using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;

namespace UKPS.Api.Tests.Services;

internal sealed class TestDatabase : IAsyncDisposable
{
    public AppDbContext Context { get; }

    public TestDatabase()
    {
        Context = new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options
        );
    }

    public ValueTask DisposeAsync()
    {
        return Context.DisposeAsync();
    }
}
