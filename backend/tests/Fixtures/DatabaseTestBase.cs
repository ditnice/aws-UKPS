using UKPS.Api.Data;

namespace UKPS.Api.Tests.Fixtures;

public abstract class DatabaseTestBase : IAsyncLifetime
{
    protected DatabaseTestBase(PostgresFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        Fixture = fixture;
        Context = fixture.CreateContext();
    }

    protected PostgresFixture Fixture { get; }

    internal AppDbContext Context { get; }

    public async ValueTask InitializeAsync() => await Fixture.ResetDatabaseAsync();

    protected async Task<T> AddEntity<T>(T entity, CancellationToken cancellationToken)
        where T : class
    {
        Context.Add(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected async Task<IEnumerable<T>> AddEntities<T>(
        IEnumerable<T> entities,
        CancellationToken cancellationToken
    )
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
        await Context.SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
