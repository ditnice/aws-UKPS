using System.Security.Claims;
using Bogus;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using UKPS.Api.Controllers.Utilities;
using UKPS.Api.Enums;
using UKPS.Api.Services.Interfaces;

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
            // Arrange
            var organisationId = faker.Random.Int(min: 0);
            var service = CreateService(
                new Claim(UkpsClaimTypes.OrganisationId, $"{organisationId}"),
                new Claim(UkpsClaimTypes.UserRole, userRole.ToString())
            );

            // Act
            CurrentUser result = service.GetCurrentUserInfo();

            // Assert
            result.OrganisationId.ShouldBe(organisationId);
            result.UserRole.ShouldBe(userRole);
        }
    }

    [Fact]
    public void GetCurrentUserInfo_ShouldThrow_WhenOrganisationIdClaimIsMissing()
    {
        // Arrange
        var service = CreateService(
            new Claim(UkpsClaimTypes.UserRole, UserRole.Champion.ToString())
        );

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => service.GetCurrentUserInfo());

        // Assert
        exception.Message.ShouldBe($"Invalid {UkpsClaimTypes.OrganisationId} claim value.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1.5")]
    public void GetCurrentUserInfo_ShouldThrow_WhenOrganisationIdClaimIsInvalid(string claimValue)
    {
        // Arrange
        var service = CreateService(
            new Claim(UkpsClaimTypes.OrganisationId, claimValue),
            new Claim(UkpsClaimTypes.UserRole, UserRole.Super.ToString())
        );

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => service.GetCurrentUserInfo());

        // Assert
        exception.Message.ShouldBe($"Invalid {UkpsClaimTypes.OrganisationId} claim value.");
    }

    [Fact]
    public void GetCurrentUserInfo_ShouldThrow_WhenUserRoleClaimIsMissing()
    {
        // Arrange
        var service = CreateService(new Claim(UkpsClaimTypes.OrganisationId, "123"));

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => service.GetCurrentUserInfo());

        // Assert
        exception.Message.ShouldBe($"Invalid {UkpsClaimTypes.UserRole} claim value.");
    }

    [Theory]
    // [InlineData("")]
    // [InlineData("Administrator")]
    [InlineData("123")]
    public void GetCurrentUserInfo_ShouldThrow_WhenUserRoleClaimIsInvalid(string claimValue)
    {
        // Arrange
        var service = CreateService(
            new Claim(UkpsClaimTypes.OrganisationId, "123"),
            new Claim(UkpsClaimTypes.UserRole, claimValue)
        );

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => service.GetCurrentUserInfo());

        // Assert
        exception.Message.ShouldBe($"Invalid {UkpsClaimTypes.UserRole} claim value.");
    }

    [Fact]
    public void GetCurrentUserInfo_ShouldThrow_WhenHttpContextIsNull()
    {
        // Arrange
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var service = new WebApiCurrentUserInfoService(accessor);

        // Act
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
