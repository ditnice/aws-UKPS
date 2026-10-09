using Bogus;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shouldly;
using UKPS.Api.Persistence.Configurations;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;

namespace UKPS.Api.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public class DatabaseConstraintTests : DatabaseTestBase
{
    private const string UniqueViolationSqlState = "23505";
    private readonly UserFaker _userFaker = new();
    private readonly OrganisationFaker _organisationFaker = new();
    private readonly UserOrgMembershipFaker _membershipFaker = new();

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
        AssertUniqueViolation(exception, ConstraintNames.UserMembershipUniqueUserAndOrgId);
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
        AssertUniqueViolation(exception, ConstraintNames.UserUniqueEmail);
    }

    [Fact]
    public async Task SaveChangesAsync_InvalidOrganisationReference_ThrowDbUpdateException()
    {
        UserOrgMembership entity = _membershipFaker
            .RuleFor(x => x.User, _ => _userFaker.Generate())
            .RuleFor(x => x.OrganisationId, _ => 999)
            .Generate();
        Context.Add(entity);
        DbUpdateException exception = await Should.ThrowAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync()
        );
        PostgresException postgresException =
            exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.ConstraintName.ShouldBe(
            ConstraintNames.UserMembershipRequiresOrganisation
        );
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAMembershipTestIsApprovedAndRejected_ShouldThrowDbUpdateException()
    {
        Faker<UserRegistrationRequest> faker = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, new OrganisationFaker().Generate())
            .RuleFor(x => x.RejectedAt, f => DateTime.SpecifyKind(f.Date.Past(), DateTimeKind.Utc))
            .RuleFor(x => x.ApprovedAt, f => DateTime.SpecifyKind(f.Date.Past(), DateTimeKind.Utc));
        UserRegistrationRequest entity = faker.Generate();
        Context.Add(entity);

        DbUpdateException exception = await Should.ThrowAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync()
        );
        PostgresException postgresException =
            exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.ConstraintName.ShouldBe(
            ConstraintNames.MembershipRequestsShouldNotBeApprovedAndRejected
        );
    }

    [Fact]
    public async Task SaveChangesAsync_DuplicateRequestGuid_ThrowsDbUpdateException()
    {
        Guid requestGuid = Guid.NewGuid();
        var organisation = _organisationFaker.Generate();
        var faker = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, organisation)
            .RuleFor(x => x.RequestGuid, _ => requestGuid);
        Context.UserRegistrationRequests.Add(faker.Generate());
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.UserRegistrationRequests.Add(faker.Generate());

        DbUpdateException exception = await Should.ThrowAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync(TestContext.Current.CancellationToken)
        );
        AssertUniqueViolation(exception, "ix_user_registration_requests_request_guid");
    }

    [Fact]
    public async Task SaveChangesAsync_UserRegistrationRequestWithNullResultingUserId_Succeeds()
    {
        UserRegistrationRequest entity = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, _organisationFaker.Generate())
            .Generate();
        Context.Add(entity);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.ChangeTracker.Clear();
        var foundValue = await Context.UserRegistrationRequests.SingleAsync(
            x => x.Id == entity.Id,
            TestContext.Current.CancellationToken
        );
        foundValue.ResultingUserId.ShouldBeNull();
        foundValue.ResultingUser.ShouldBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_UserRegistrationRequestWithInvalidResultingUserId_ThrowsDbUpdateException()
    {
        UserRegistrationRequest entity = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, _organisationFaker.Generate())
            .RuleFor(x => x.ResultingUserId, _ => int.MaxValue)
            .Generate();
        Context.Add(entity);

        DbUpdateException exception = await Should.ThrowAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync()
        );
        PostgresException postgresException =
            exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe("23503");
    }

    [Fact]
    public async Task SaveChangesAsync_UserOnboardingRecordWithCreatorAndResultingUser_Succeeds()
    {
        var creator = _userFaker.Generate();
        var resultingUser = _userFaker.Generate();
        UserOnboardingRecord entity = new UserOnboardingRecordFaker()
            .RuleFor(x => x.CreatedByUser, creator)
            .RuleFor(x => x.ResultingUser, resultingUser)
            .Generate();
        Context.Add(entity);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.ChangeTracker.Clear();
        var foundValue = await Context
            .UserOnboardingRecords.Include(x => x.CreatedByUser)
            .Include(x => x.ResultingUser)
            .SingleAsync(
                x => x.SetupToken == entity.SetupToken,
                TestContext.Current.CancellationToken
            );
        foundValue.CreatedByUserId.ShouldBe(creator.Id);
        foundValue.CreatedByUser.ShouldNotBeNull().Id.ShouldBe(creator.Id);
        foundValue.ResultingUserId.ShouldBe(resultingUser.Id);
        foundValue.ResultingUser.ShouldNotBeNull().Id.ShouldBe(resultingUser.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public async Task SaveChangesAsync_UserOnboardingRecordWithInvalidCreator_ThrowsDbUpdateException(
        int creatorId
    )
    {
        UserOnboardingRecord entity = new UserOnboardingRecordFaker()
            .RuleFor(x => x.CreatedByUser, _ => null)
            .RuleFor(x => x.CreatedByUserId, _ => creatorId)
            .RuleFor(x => x.ResultingUser, _userFaker.Generate())
            .Generate();
        Context.Add(entity);

        DbUpdateException exception = await Should.ThrowAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync(TestContext.Current.CancellationToken)
        );
        exception.InnerException.ShouldBeOfType<PostgresException>().SqlState.ShouldBe("23503");
    }

    [Fact]
    public async Task SaveChangesAsync_UserRegistrationRequestWithInvalidRejector_ThrowsDbUpdateException()
    {
        UserRegistrationRequest entity = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, _organisationFaker.Generate())
            .RuleFor(x => x.RejectedByUserId, _ => int.MaxValue)
            .Generate();
        Context.Add(entity);

        DbUpdateException exception = await Should.ThrowAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync(TestContext.Current.CancellationToken)
        );
        exception.InnerException.ShouldBeOfType<PostgresException>().SqlState.ShouldBe("23503");
    }

    [Fact]
    public async Task SaveChangesAsync_WhenUserHasMultipleUserOrgMembershipsMarkedAsSelected_ShouldThrowDbUpdateException()
    {
        var user = new UserFaker().Generate();
        var organisationFaker = new OrganisationFaker();
        Faker<UserOrgMembership> faker = new UserOrgMembershipFaker()
            .RuleFor(x => x.User, _ => user)
            .RuleFor(x => x.Organisation, _ => organisationFaker.Generate())
            .RuleFor(x => x.IsSelectedAsCurrentOrganisation, _ => true);

        Context.AddRange(faker.Generate(2));

        DbUpdateException exception = await Should.ThrowAsync<DbUpdateException>(() =>
            Context.SaveChangesAsync()
        );
        PostgresException postgresException =
            exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.ConstraintName.ShouldBe(
            ConstraintNames.UsersCannotHaveMultipleSelectedCurrentOrganisations
        );
    }

    private static void AssertUniqueViolation(DbUpdateException exception, string constraintName)
    {
        PostgresException postgresException =
            exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe(UniqueViolationSqlState);
        postgresException.ConstraintName.ShouldBe(constraintName);
    }
}
