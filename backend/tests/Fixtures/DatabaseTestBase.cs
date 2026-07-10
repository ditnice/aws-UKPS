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

    protected async Task<T> AddEntity<T>(T entity)
        where T : class
    {
        Context.Add(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    protected async Task<IEnumerable<T>> AddEntities<T>(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        foreach (var entity in entities)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(
                    nameof(entities),
                    "Entity in collection cannot be null"
                );
            }
            Context.Add(entity);
        }
        await Context.SaveChangesAsync();
        return entities;
    }

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
