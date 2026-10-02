using Shouldly;
using UKPS.Api.Persistence.Data.Seeding;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Persistence;

public sealed class DataSeederInMemoryTests
{
    [Fact]
    public void BuildPayload_WhenSeedUsersJsonIsEmpty_ShouldKeepGeneratedSeedData()
    {
        SeedingDataPayload payload = DataSeederInMemory.BuildPayload(new SeedingOptions());

        payload.Organisations.Count.ShouldBe(5);
        payload.Users.Count.ShouldBe(80);
        payload.Memberships.Count.ShouldBe(81); //Additional membership for dev user
        payload.Records.Count.ShouldBe(20);
    }

    [Fact]
    public void BuildPayload_WhenSeedUsersJsonHasSuperUser_ShouldAddSuperUserForOrganisationOne()
    {
        const string email = "bootstrap.user@example.com";
        const string cognitoUsername = "00000000-0000-0000-0000-000000000001";
        SeedingOptions configuration = new()
        {
            SeedUsersJson = """
                [
                  {
                    "fullName": "Bootstrap User",
                    "email": "bootstrap.user@example.com",
                    "cognitoUsername": "00000000-0000-0000-0000-000000000001",
                    "role": "Super"
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
        membership.Organisation.ShouldBeSameAs(payload.Organisations.First());
        membership.UserRole.ShouldBe(UserRole.Super);
        membership.Status.ShouldBe(UserOrgMembershipStatus.Active);
        membership.AllowedPharmaceuticalEntity.ShouldBe(PharmaceuticalEntity.Both);
    }

    [Fact]
    public void BuildPayload_WhenSuperUserMatchesGeneratedEmail_ShouldReplaceGeneratedUser()
    {
        SeedingDataPayload generatedPayload = DataSeederInMemory.BuildPayload(new SeedingOptions());
        string generatedEmail = generatedPayload.Users.First().WorkEmail;
        const string cognitoUsername = "00000000-0000-0000-0000-000000000002";
        SeedingOptions configuration = new()
        {
            SeedUsersJson = $$"""
                [
                  {
                    "fullName": "Configured User",
                    "email": "{{generatedEmail}}",
                    "cognitoUsername": "00000000-0000-0000-0000-000000000002",
                    "role": "Super"
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

    [Theory]
    [InlineData("Standard", UserRole.Standard)]
    [InlineData("champion", UserRole.Champion)]
    public void BuildPayload_WhenSeedUsersJsonHasNonSuperRole_ShouldAddPharmaUserToFourOrganisations(
        string role,
        UserRole expectedRole
    )
    {
        const string email = "seeded.user@example.com";
        SeedingOptions configuration = new()
        {
            SeedUsersJson = $$"""
                [
                  {
                    "fullName": "Seeded User",
                    "email": "{{email}}",
                    "cognitoUsername": "00000000-0000-0000-0000-000000000003",
                    "role": "{{role}}"
                  }
                ]
                """,
        };

        SeedingDataPayload payload = DataSeederInMemory.BuildPayload(configuration);

        User user = payload.Users.Single(u =>
            string.Equals(u.WorkEmail, email, StringComparison.Ordinal)
        );
        user.UserType.ShouldBe(UserType.PharmaUser);

        Organisation[] expectedOrganisations = payload
            .Organisations.Take(DataSeederInMemory.NonSuperUserOrganisationCount)
            .ToArray();
        expectedOrganisations.Length.ShouldBe(4);
        expectedOrganisations.ShouldAllBe(o => o.Status == UserOrgStatus.Active);

        UserOrgMembership[] memberships = payload
            .Memberships.Where(m => ReferenceEquals(m.User, user))
            .ToArray();
        memberships.Select(m => m.Organisation).ShouldBe(expectedOrganisations);
        memberships.ShouldNotContain(m =>
            ReferenceEquals(m.Organisation, payload.Organisations.Last())
        );
        foreach (UserOrgMembership membership in memberships)
        {
            membership.UserRole.ShouldBe(expectedRole);
            membership.Status.ShouldBe(UserOrgMembershipStatus.Active);
            membership.AllowedPharmaceuticalEntity.ShouldBe(PharmaceuticalEntity.Both);
        }
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("1")]
    [InlineData("")]
    public void BuildPayload_WhenSeedUsersJsonHasInvalidRole_ShouldThrow(string role)
    {
        SeedingOptions configuration = new()
        {
            SeedUsersJson = $$"""
                [
                  {
                    "fullName": "Seeded User",
                    "email": "seeded.user@example.com",
                    "cognitoUsername": "00000000-0000-0000-0000-000000000003",
                    "role": "{{role}}"
                  }
                ]
                """,
        };

        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
            DataSeederInMemory.BuildPayload(configuration)
        );
        exception.Message.ShouldContain("must include a valid role");
    }

    [Fact]
    public void BuildPayload_WhenSeedUsersJsonHasDuplicateEmails_ShouldThrow()
    {
        SeedingOptions configuration = new()
        {
            SeedUsersJson = """
                [
                  {
                    "fullName": "Bootstrap User One",
                    "email": "bootstrap.user@example.com",
                    "cognitoUsername": "00000000-0000-0000-0000-000000000001",
                    "role": "Super"
                  },
                  {
                    "fullName": "Bootstrap User Two",
                    "email": "BOOTSTRAP.USER@example.com",
                    "cognitoUsername": "00000000-0000-0000-0000-000000000002",
                    "role": "Super"
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
    public void BuildPayload_WhenSeedUsersJsonIsInvalidJson_ShouldThrow()
    {
        SeedingOptions configuration = new() { SeedUsersJson = "not-json" };

        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
            DataSeederInMemory.BuildPayload(configuration)
        );
        exception.Message.ShouldBe("Seeding users must be a valid JSON array.");
    }
}
