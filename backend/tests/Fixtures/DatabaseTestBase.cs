using UKPS.Api.Data;

namespace UKPS.Api.Tests.Fixtures;

public abstract class DatabaseTestBase : IAsyncLifetime, IAsyncDisposable
{
    protected DatabaseTestBase(PostgresFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        Fixture = fixture;
        Context = fixture.CreateContext();
    }

    protected PostgresFixture Fixture { get; }

    internal AppDbContext Context { get; }

    public Task InitializeAsync() => Fixture.ResetDatabaseAsync();

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
