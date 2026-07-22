using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shouldly;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;

namespace UKPS.Api.Tests.Persistence;

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
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var membership = _membershipFaker
            .Generate()
            .Update(x =>
            {
                x.UserId = user.Id;
                x.OrganisationId = organisation.Id;
            });
        Context.UserOrgMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.UserOrgMemberships.Add(membership.Update(x => x.Id = 2));

        DbUpdateException exception = await Should.ThrowAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync()
        );
        AssertUniqueViolation(exception);
    }

    [Fact]
    public async Task SaveChangesAsync_DuplicateUsername_ThrowsDbUpdateException()
    {
        var duplicateUsername = "duplicate-name";
        var user1 = _userFaker.Generate().Update(x => x.Username = duplicateUsername);
        var user2 = _userFaker.Generate().Update(x => x.Username = duplicateUsername);
        Context.Users.Add(user1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.Users.Add(user2);

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync(TestContext.Current.CancellationToken)
        );
        AssertUniqueViolation(exception);
    }

    [Fact]
    public async Task SaveChangesAsync_DuplicateWorkEmail_ThrowsDbUpdateException()
    {
        var duplicateEmail = "duplicate@example.com";
        var user1 = _userFaker.Generate().Update(x => x.WorkEmail = duplicateEmail);
        var user2 = _userFaker.Generate().Update(x => x.WorkEmail = duplicateEmail);
        Context.Users.Add(user1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.Users.Add(user2);

        DbUpdateException exception = await Should.ThrowAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync()
        );
        AssertUniqueViolation(exception);
    }

    private static void AssertUniqueViolation(DbUpdateException exception)
    {
        PostgresException postgresException =
            exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe(UniqueViolationSqlState);
    }
}
