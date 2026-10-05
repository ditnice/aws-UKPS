using System.Security.Claims;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.Tests.Utilities.Fixtures;

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

    public virtual async ValueTask InitializeAsync()
    {
        // The API factory is shared across the collection, so restore the default caller in
        // case a previous test changed it.
        ResetAuthenticatedClaims(TestAuthenticationOptions.DefaultClaims);
        await Fixture.ResetDatabaseAsync();
    }

    /// <summary>
    /// Changes the role and organisation of the caller used for subsequent API requests.
    /// </summary>
    protected void AuthenticateAs(UserRole role, int organisationId)
    {
        ResetAuthenticatedClaims(
            TestAuthenticationOptions
                .DefaultClaims.Where(c =>
                    c.Type is not (UkpsClaimTypes.UserRole or UkpsClaimTypes.OrganisationId)
                )
                .Append(new Claim(UkpsClaimTypes.UserRole, role.ToString()))
                .Append(new Claim(UkpsClaimTypes.OrganisationId, $"{organisationId}"))
        );
    }

    private void ResetAuthenticatedClaims(IEnumerable<Claim> claims)
    {
        ICollection<Claim> current = Fixture.Factory.AuthOptions.Claims;
        current.Clear();
        foreach (Claim claim in claims)
        {
            current.Add(claim);
        }
    }

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
