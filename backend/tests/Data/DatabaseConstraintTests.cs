using Microsoft.EntityFrameworkCore;
using Npgsql;
using UKPS.Api.Enums;
using UKPS.Api.Tests.Fixtures;

namespace UKPS.Api.Tests.Data;

[Collection(DatabaseCollection.Name)]
public class DatabaseConstraintTests : DatabaseTestBase
{
    private const string UniqueViolationSqlState = "23505";

    public DatabaseConstraintTests(PostgresFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task SaveChangesAsync_DuplicateMembershipKey_ThrowsDbUpdateException()
    {
        // Both FKs are Restrict, so the parent User and Organisation rows must exist first.
        Context.Users.Add(EntityFactory.CreateUser(id: 1, workEmail: "member@example.com"));
        Context.Organisations.Add(EntityFactory.CreateOrganisation(id: 1));
        await Context.SaveChangesAsync();

        Context.UserOrgMemberships.Add(
            EntityFactory.CreateMembership(
                id: 1,
                userId: 1,
                organisationId: 1,
                allowedPharmaceuticalEntity: PharmaceuticalEntity.Both
            )
        );
        await Context.SaveChangesAsync();

        Context.UserOrgMemberships.Add(
            EntityFactory.CreateMembership(
                id: 2,
                userId: 1,
                organisationId: 1,
                allowedPharmaceuticalEntity: PharmaceuticalEntity.Both
            )
        );

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync()
        );
        AssertUniqueViolation(exception);
    }

    [Fact]
    public async Task SaveChangesAsync_DuplicateUsername_ThrowsDbUpdateException()
    {
        Context.Users.Add(EntityFactory.CreateUser(id: 1, workEmail: "duplicate@example.com"));
        await Context.SaveChangesAsync();

        Context.Users.Add(EntityFactory.CreateUser(id: 2, workEmail: "duplicate@example.com"));

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync()
        );
        AssertUniqueViolation(exception);
    }

    private static void AssertUniqueViolation(DbUpdateException exception)
    {
        PostgresException postgresException = Assert.IsType<PostgresException>(
            exception.InnerException
        );
        Assert.Equal(UniqueViolationSqlState, postgresException.SqlState);
    }
}
