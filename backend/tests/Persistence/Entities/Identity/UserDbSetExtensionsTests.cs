using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Tests.Utilities.Fixtures;

namespace UKPS.Api.Tests.Persistence.Entities.Identity;

[Collection(DatabaseCollection.Name)]
public class UserDbSetExtensionsTests : DatabaseTestBase
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public UserDbSetExtensionsTests(PostgresFixture postgresFixture)
        : base(postgresFixture) { }

    [Fact]
    public async Task GetByEmailOrDefault_WhenEmailMatchesExactly_ShouldReturnUser()
    {
        await Seed("example@email.com");

        var result = await Lookup("example@email.com");

        result.ShouldNotBeNull();
        result.WorkEmail.ShouldBe("example@email.com");
    }

    [Fact]
    public async Task GetByEmailOrDefault_WhenNoUserMatches_ShouldReturnNull()
    {
        await Seed("example@email.com");

        var result = await Lookup("someone.else@email.com");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByEmailOrDefault_WhenTableIsEmpty_ShouldReturnNull()
    {
        var result = await Lookup("example@email.com");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByEmailOrDefault_WhenSeveralUsersExist_ShouldReturnTheMatchingUser()
    {
        await Seed("first@email.com");
        var second = await Seed("second@email.com");
        await Seed("third@email.com");

        var result = await Lookup("SECOND@email.com");

        result.ShouldNotBeNull();
        result.Id.ShouldBe(second.Id);
    }

    [Fact]
    public async Task GetByEmailOrDefault_WhenEmailIsEmptyString_ShouldReturnNull()
    {
        await Seed("example@email.com");

        var result = await Lookup("");

        result.ShouldBeNull();
    }

    // ---------- Normalised column ----------

    [Theory]
    [InlineData("example@email.com", "EXAMPLE@EMAIL.COM")]
    [InlineData("Example@Email.Com", "EXAMPLE@EMAIL.COM")]
    [InlineData("EXAMPLE@EMAIL.COM", "EXAMPLE@EMAIL.COM")]
    [InlineData("josé@email.com", "JOSé@EMAIL.COM")] // C collation: only ASCII is folded
    public async Task GetByEmailOrDefault_WhenUserIsStored_ShouldComputeNormalisedColumnInDatabase(
        string stored,
        string expected
    )
    {
        await Seed(stored);

        var normalised = await Context
            .Users.AsNoTracking()
            .Where(x => x.WorkEmail == stored)
            .Select(x => x.NormalisedWorkEmail)
            .SingleAsync(Ct);

        normalised.ShouldBe(expected);
    }

    [Theory]
    [InlineData("EXAMPLE@EMAIL.COM")]
    [InlineData("Example@Email.com")]
    [InlineData("eXaMpLe@eMaIl.CoM")]
    public async Task GetByEmailOrDefault_WhenAsciiCaseDiffers_ShouldReturnUser(string searchFor)
    {
        await Seed("example@email.com");

        var result = await Lookup(searchFor);

        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByEmailOrDefault_WhenStoredEmailHasMixedCase_ShouldReturnUser()
    {
        await Seed("Jane.Doe@Example.com");

        var result = await Lookup("jane.doe@example.com");

        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByEmailOrDefault_WhenSearchContainsInnerWhitespace_ShouldReturnNull()
    {
        await Seed("example@email.com");

        var result = await Lookup("exa mple@email.com");

        result.ShouldBeNull();
    }

    // ---------- Non-ASCII behaviour (documents the "C" collation choice) ----------

    [Fact]
    public async Task GetByEmailOrDefault_WhenOnlyAsciiCaseDiffersOnNonAsciiEmail_ShouldReturnUser()
    {
        await Seed("josé@email.com");

        var result = await Lookup("JOSé@EMAIL.COM");

        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByEmailOrDefault_WhenNonAsciiLetterCaseDiffers_ShouldReturnNull()
    {
        await Seed("josé@email.com");

        var result = await Lookup("JOSÉ@EMAIL.COM");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByEmailOrDefault_WhenSharpSIsSearchedAsSS_ShouldReturnNull()
    {
        await Seed("straße@email.com");

        (await Lookup("STRASSE@EMAIL.COM")).ShouldBeNull();
        (await Lookup("STRAßE@EMAIL.COM")).ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByEmailOrDefault_WhenTurkishDotlessIIsSearchedAsAsciiI_ShouldReturnNull()
    {
        await Seed("kızıl@email.com"); // contains U+0131

        (await Lookup("KIZIL@EMAIL.COM")).ShouldBeNull();
        (await Lookup("KıZıL@EMAIL.COM")).ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByEmailOrDefault_WhenAccentIsComposedVersusDecomposed_ShouldReturnNull()
    {
        // The DB function does no Unicode normalisation. If you normalise to FormC
        // at the API boundary, this test should stay as a reminder that you must.
        await Seed("jos\u00E9@email.com"); // é as one code point

        var result = await Lookup("jose\u0301@email.com"); // e + combining acute

        result.ShouldBeNull();
    }

    private async Task<User> Seed(string workEmail)
    {
        User user = new UserFaker().RuleFor(x => x.WorkEmail, _ => workEmail).Generate();
        await AddEntity(user, Ct);
        return user;
    }

    private Task<User?> Lookup(string email) =>
        Context.Users.AsNoTracking().GetByEmailOrDefault(email, Ct);
}
