using System.Security.Claims;
using Bogus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.Tests.WebApi;

[Collection(DatabaseCollection.Name)]
public sealed class TokenValidationHandlerTests : DatabaseTestBase
{
    private const string ClientId = "test-client-id";

    private readonly User _user;
    private readonly User _userWithMultipleMemberships;
    private UserOrgMembership UserMembership => _user.UserOrgMemberships.Single();
    private readonly Faker<Organisation> _orgFaker;
    private readonly Faker<UserOrgMembership> _membershipFaker;
    private readonly Faker<User> _userFaker;

    private readonly TokenValidationHandler _handler;

    public TokenValidationHandlerTests(PostgresFixture fixture)
        : base(fixture)
    {
        _orgFaker = new OrganisationFaker();
        _membershipFaker = new UserOrgMembershipFaker().RuleFor(
            x => x.Organisation,
            _ => _orgFaker.Generate()
        );
        _userFaker = new UserFaker().RuleFor(
            x => x.UserOrgMemberships,
            _ => _membershipFaker.Generate(1)
        );
        _user = _userFaker.Generate();
        var options = Options.Create(
            new CognitoConfiguration
            {
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret",
                Region = "eu-west-2",
                UserPoolId = "eu-west-2_test123",
            }
        );
        _userWithMultipleMemberships = _userFaker.RuleFor(
            x => x.UserOrgMemberships,
            _ => _membershipFaker.Generate(3)
        );
        _handler = new TokenValidationHandler(Context, options);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await AddEntity(_user, TestContext.Current.CancellationToken);
        await AddEntity(_userWithMultipleMemberships, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldAppendClaims_WhenTokenIsValidAndUserHasSingleMembership()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: ClientId,
            subject: _user.IdentityId
        );

        await _handler.Handle(context, CancellationToken.None);

        context.Result.ShouldBeNull();

        var identity = context.Principal!.Identity.ShouldBeOfType<ClaimsIdentity>();

        AssertIdentityMatchesUserAndMembership(identity, _user, UserMembership);
    }

    private static void AssertIdentityMatchesUserAndMembership(
        ClaimsIdentity identity,
        User user,
        UserOrgMembership userMembership
    )
    {
        identity.FindFirst(UkpsClaimTypes.Email)?.Value.ShouldBe(user.WorkEmail);
        identity
            .FindFirst(UkpsClaimTypes.OrganisationId)
            ?.Value.ShouldBe($"{userMembership.OrganisationId}");
        identity.FindFirst(UkpsClaimTypes.UserRole)?.Value.ShouldBe($"{userMembership.UserRole}");
    }

    [Fact]
    public async Task Handle_ShouldAppendClaimsForSelectedOrganisation_WhenUserHasMultipleMemberships()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: ClientId,
            subject: _userWithMultipleMemberships.IdentityId
        );

        var selectedMembership = _userWithMultipleMemberships.UserOrgMemberships.ElementAt(1);
        context.HttpContext.Request.Cookies = CreateCookieCollection(
            ("selected_organisation", $"{selectedMembership.OrganisationId}")
        );

        await _handler.Handle(context, CancellationToken.None);

        context.Result.ShouldBeNull();

        var identity = context.Principal!.Identity.ShouldBeOfType<ClaimsIdentity>();

        AssertIdentityMatchesUserAndMembership(
            identity,
            _userWithMultipleMemberships,
            selectedMembership
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTokenUseIsNotAccess()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "id",
            clientId: ClientId,
            subject: _user.IdentityId
        );

        await _handler.Handle(context, CancellationToken.None);

        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe("Token is not an access token.");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTokenUseIsMissing()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: null,
            clientId: ClientId,
            subject: _user.IdentityId
        );

        // Act
        await _handler.Handle(context, CancellationToken.None);

        // Assert
        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe("Token is not an access token.");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenClientIdIsInvalid()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: "wrong-client-id",
            subject: _user.IdentityId
        );

        // Act
        await _handler.Handle(context, CancellationToken.None);

        // Assert
        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe("Token was not issued to the expected client.");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenClientIdIsMissing()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: null,
            subject: _user.IdentityId
        );

        // Act
        await _handler.Handle(context, CancellationToken.None);

        // Assert
        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe("Token was not issued to the expected client.");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExist()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: ClientId,
            subject: "none-existent-user-id"
        );

        await _handler.Handle(context, CancellationToken.None);

        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe(
            "No user exists in the database with the given identity ID"
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMultipleMembershipsAndOrganisationCookieIsMissing()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: ClientId,
            subject: _userWithMultipleMemberships.IdentityId
        );

        await _handler.Handle(context, CancellationToken.None);

        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe(
            "A valid selected organisation cookie is required."
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSelectedOrganisationCookieIsNotAnInteger()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: ClientId,
            subject: _userWithMultipleMemberships.IdentityId
        );

        context.HttpContext.Request.Cookies = CreateCookieCollection(
            ("selected_organisation", "not-an-integer")
        );

        await _handler.Handle(context, CancellationToken.None);

        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe(
            "A valid selected organisation cookie is required."
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSelectedOrganisationDoesNotBelongToUser()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: ClientId,
            subject: _userWithMultipleMemberships.IdentityId
        );

        context.HttpContext.Request.Cookies = CreateCookieCollection(
            ("selected_organisation", "999")
        );

        await _handler.Handle(context, CancellationToken.None);

        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe(
            "The selected organisation is not associated with the user."
        );
    }

    [Fact]
    public async Task Handle_ShouldNotAppendClaims_WhenMembershipSelectionFails()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: ClientId,
            subject: _userWithMultipleMemberships.IdentityId
        );

        context.HttpContext.Request.Cookies = CreateCookieCollection(
            ("selected_organisation", "999")
        );

        await _handler.Handle(context, CancellationToken.None);

        var identity = context.Principal!.Identity.ShouldBeOfType<ClaimsIdentity>();

        identity.FindFirst(UkpsClaimTypes.Email).ShouldBeNull();
        identity.FindFirst(UkpsClaimTypes.OrganisationId).ShouldBeNull();
        identity.FindFirst(UkpsClaimTypes.UserRole).ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenIdentitySubCannotBeFound()
    {
        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: ClientId,
            subject: null
        );
        await Should.ThrowAsync<InvalidOperationException>(() =>
        {
            return _handler.Handle(context, CancellationToken.None);
        });
    }

    [Fact]
    public async Task Handle_ShouldNotAddClaims_WhenUserHasNoMemberships()
    {
        var userWithNoMembership = _userFaker
            .RuleFor(x => x.UserOrgMemberships, _ => [])
            .Generate();
        await AddEntity(userWithNoMembership, TestContext.Current.CancellationToken);

        var context = CreateTokenValidatedContext(
            tokenUse: "access",
            clientId: ClientId,
            subject: userWithNoMembership.IdentityId
        );

        await _handler.Handle(context, CancellationToken.None);

        var identity = context.Principal!.Identity.ShouldBeOfType<ClaimsIdentity>();

        identity.FindFirst(UkpsClaimTypes.Email).ShouldBeNull();
        identity.FindFirst(UkpsClaimTypes.OrganisationId).ShouldBeNull();
        identity.FindFirst(UkpsClaimTypes.UserRole).ShouldBeNull();
    }

    private static IRequestCookieCollection CreateCookieCollection(
        params (string Name, string Value)[] cookies
    )
    {
        var context = new DefaultHttpContext();

        context.Request.Headers.Cookie = string.Join(
            "; ",
            cookies.Select(x => $"{x.Name}={x.Value}")
        );

        return context.Request.Cookies;
    }

    private static TokenValidatedContext CreateTokenValidatedContext(
        string? tokenUse,
        string? clientId,
        string? subject
    )
    {
        var claims = new List<Claim>();

        if (tokenUse is not null)
        {
            claims.Add(new Claim("token_use", tokenUse));
        }

        if (clientId is not null)
        {
            claims.Add(new Claim("client_id", clientId));
        }

        if (subject is not null)
        {
            claims.Add(new Claim("sub", subject));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

        var httpContext = new DefaultHttpContext();

        var authenticationScheme = new AuthenticationScheme(
            "Bearer",
            "Bearer",
            typeof(JwtBearerHandler)
        );

        return new TokenValidatedContext(httpContext, authenticationScheme, new JwtBearerOptions())
        {
            Principal = principal,
        };
    }
}
