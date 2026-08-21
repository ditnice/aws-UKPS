using Shouldly;
using UKPS.Api.Persistence.Data.Seeding;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Persistence;

public sealed class DataSeederInMemoryTests
{
    [Fact]
    public void BuildPayload_WhenSuperUsersJsonIsEmpty_ShouldKeepGeneratedSeedData()
    {
        SeedingDataPayload payload = DataSeederInMemory.BuildPayload(new SeedingConfiguration());

        payload.Organisations.Count.ShouldBe(5);
        payload.Users.Count.ShouldBe(80);
        payload.Memberships.Count.ShouldBe(80);
    }

    [Fact]
    public void BuildPayload_WhenSuperUsersJsonIsConfigured_ShouldAddSuperUserForOrganisationOne()
    {
        const string email = "bootstrap.user@example.com";
        const string cognitoUsername = "00000000-0000-0000-0000-000000000001";
        SeedingConfiguration configuration = new()
        {
            SuperUsersJson = """
                [
                  {
                    "fullName": "Bootstrap User",
                    "email": "bootstrap.user@example.com",
                    "cognitoUsername": "00000000-0000-0000-0000-000000000001"
                  }
                ]
                """,
        };

        SeedingDataPayload payload = DataSeederInMemory.BuildPayload(configuration);

        User user = payload.Users.Single(u =>
            string.Equals(u.WorkEmail, email, StringComparison.Ordinal)
        );
        user.FullName.ShouldBe("Bootstrap User");
        user.CognitoUsername.ShouldBe(CognitoUsername.Parse(cognitoUsername));
        user.UserType.ShouldBe(UserType.ItAdmin);
        payload.Organisations.First().Status.ShouldBe(UserOrgStatus.Active);

        UserOrgMembership membership = payload.Memberships.Single(m =>
            ReferenceEquals(m.User, user)
        );
        membership.OrganisationId.ShouldBe(1);
        membership.UserRole.ShouldBe(UserRole.Super);
        membership.Status.ShouldBe(UserOrgStatus.Active);
        membership.AllowedPharmaceuticalEntity.ShouldBe(PharmaceuticalEntity.Both);
    }

    [Fact]
    public void BuildPayload_WhenSuperUserMatchesGeneratedEmail_ShouldReplaceGeneratedUser()
    {
        SeedingDataPayload generatedPayload = DataSeederInMemory.BuildPayload(
            new SeedingConfiguration()
        );
        string generatedEmail = generatedPayload.Users.First().WorkEmail;
        const string cognitoUsername = "00000000-0000-0000-0000-000000000002";
        SeedingConfiguration configuration = new()
        {
            SuperUsersJson = $$"""
                [
                  {
                    "fullName": "Configured User",
                    "email": "{{generatedEmail}}",
                    "cognitoUsername": "00000000-0000-0000-0000-000000000002"
                  }
                ]
                """,
        };

        SeedingDataPayload payload = DataSeederInMemory.BuildPayload(configuration);

        User user = payload.Users.Single(u =>
            string.Equals(u.WorkEmail, generatedEmail, StringComparison.Ordinal)
        );
        user.FullName.ShouldBe("Configured User");
        user.CognitoUsername.ShouldBe(CognitoUsername.Parse(cognitoUsername));
        payload.Memberships.Count(m => ReferenceEquals(m.User, user)).ShouldBe(1);
        payload
            .Memberships.Single(m => ReferenceEquals(m.User, user))
            .UserRole.ShouldBe(UserRole.Super);
    }

    [Fact]
    public void BuildPayload_WhenSuperUsersJsonHasDuplicateEmails_ShouldThrow()
    {
        SeedingConfiguration configuration = new()
        {
            SuperUsersJson = """
                [
                  {
                    "fullName": "Bootstrap User One",
                    "email": "bootstrap.user@example.com",
                    "cognitoUsername": "00000000-0000-0000-0000-000000000001"
                  },
                  {
                    "fullName": "Bootstrap User Two",
                    "email": "BOOTSTRAP.USER@example.com",
                    "cognitoUsername": "00000000-0000-0000-0000-000000000002"
                  }
                ]
                """,
        };

        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
            DataSeederInMemory.BuildPayload(configuration)
        );
        exception.Message.ShouldContain("duplicate emails");
    }

    [Fact]
    public void BuildPayload_WhenSuperUsersJsonIsInvalidJson_ShouldThrow()
    {
        SeedingConfiguration configuration = new() { SuperUsersJson = "not-json" };

        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
            DataSeederInMemory.BuildPayload(configuration)
        );
        exception.Message.ShouldBe("Seeding super users must be a valid JSON array.");
    }
}
