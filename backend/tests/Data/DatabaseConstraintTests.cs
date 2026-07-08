using Microsoft.EntityFrameworkCore;
using Npgsql;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.Data.Fakers;

namespace UKPS.Api.Tests.Data;

[Collection(DatabaseCollection.Name)]
public class DatabaseConstraintTests : DatabaseTestBase
{
    private const string UniqueViolationSqlState = "23505";
    private readonly UserFaker _userFaker = new UserFaker();
    private readonly OrganisationFaker _organisationFaker = new OrganisationFaker();
    private readonly UserOrgMembershipFaker _membershipFaker = new UserOrgMembershipFaker();

    public DatabaseConstraintTests(PostgresFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task SaveChangesAsync_DuplicateMembershipKey_ThrowsDbUpdateException()
    {
        // Both FKs are Restrict, so the parent User and Organisation rows must exist first.
        var user = _userFaker.Generate();
        Context.Users.Add(user);
        var organisation = _organisationFaker.Generate();
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync();

        var membership = _membershipFaker.Generate() with
        {
            UserId = user.Id,
            OrganisationId = organisation.Id,
        };
        Context.UserOrgMemberships.Add(membership);
        await Context.SaveChangesAsync();

        Context.UserOrgMemberships.Add(membership with { Id = 2 });

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync()
        );
        AssertUniqueViolation(exception);
    }

    [Fact]
    public async Task SaveChangesAsync_DuplicateUsername_ThrowsDbUpdateException()
    {
        var duplicateEmail = "duplicate@example.com";
        var user1 = _userFaker.Generate() with { WorkEmail = duplicateEmail };
        var user2 = _userFaker.Generate() with { WorkEmail = duplicateEmail };
        Context.Users.Add(user1);
        await Context.SaveChangesAsync();

        Context.Users.Add(user2);

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
