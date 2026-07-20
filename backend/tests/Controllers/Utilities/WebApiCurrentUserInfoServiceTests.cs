using System.Security.Claims;
using Bogus;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.Utilities;

namespace UKPS.Api.Tests.Controllers.Utilities;

public class WebApiCurrentUserInfoServiceTests
{
    [Fact]
    public void GetCurrentUserInfo_ShouldReturnCurrentUser_WhenClaimsAreValid()
    {
        Randomizer.Seed = new Random(0);
        var faker = new Faker();
        foreach (var userRole in Enum.GetValues<UserRole>())
        {
            var organisationId = faker.Random.Int(min: 0);
            var service = CreateService(
                new Claim(UkpsClaimTypes.OrganisationId, $"{organisationId}"),
                new Claim(UkpsClaimTypes.UserRole, userRole.ToString())
            );

            CurrentUser result = service.GetCurrentUserInfo();

            result.OrganisationId.ShouldBe(organisationId);
            result.UserRole.ShouldBe(userRole);
        }
    }

    [Fact]
    public void GetCurrentUserInfo_ShouldThrow_WhenOrganisationIdClaimIsMissing()
    {
        var service = CreateService(
            new Claim(UkpsClaimTypes.UserRole, UserRole.Champion.ToString())
        );

        var exception = Should.Throw<InvalidOperationException>(() => service.GetCurrentUserInfo());

        exception.Message.ShouldBe($"Invalid {UkpsClaimTypes.OrganisationId} claim value.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1.5")]
    public void GetCurrentUserInfo_ShouldThrow_WhenOrganisationIdClaimIsInvalid(string claimValue)
    {
        var service = CreateService(
            new Claim(UkpsClaimTypes.OrganisationId, claimValue),
            new Claim(UkpsClaimTypes.UserRole, UserRole.Super.ToString())
        );

        var exception = Should.Throw<InvalidOperationException>(() => service.GetCurrentUserInfo());

        exception.Message.ShouldBe($"Invalid {UkpsClaimTypes.OrganisationId} claim value.");
    }

    [Fact]
    public void GetCurrentUserInfo_ShouldThrow_WhenUserRoleClaimIsMissing()
    {
        var service = CreateService(new Claim(UkpsClaimTypes.OrganisationId, "123"));

        var exception = Should.Throw<InvalidOperationException>(() => service.GetCurrentUserInfo());

        exception.Message.ShouldBe($"Invalid {UkpsClaimTypes.UserRole} claim value.");
    }

    [Theory]
    [InlineData("123")]
    public void GetCurrentUserInfo_ShouldThrow_WhenUserRoleClaimIsInvalid(string claimValue)
    {
        var service = CreateService(
            new Claim(UkpsClaimTypes.OrganisationId, "123"),
            new Claim(UkpsClaimTypes.UserRole, claimValue)
        );

        var exception = Should.Throw<InvalidOperationException>(() => service.GetCurrentUserInfo());

        exception.Message.ShouldBe($"Invalid {UkpsClaimTypes.UserRole} claim value.");
    }

    [Fact]
    public void GetCurrentUserInfo_ShouldThrow_WhenHttpContextIsNull()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var service = new WebApiCurrentUserInfoService(accessor);

        Should.Throw<InvalidOperationException>(() => service.GetCurrentUserInfo());
    }

    private static WebApiCurrentUserInfoService CreateService(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")),
        };

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        return new WebApiCurrentUserInfoService(accessor);
    }
}
