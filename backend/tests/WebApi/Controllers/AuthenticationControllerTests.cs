using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Tests.Utilities.Fixtures;
using LoginResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.LoginError
>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class AuthenticationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IAuthenticationService _mockedService =
        Substitute.For<IAuthenticationService>();
    private readonly HttpClient _client;
    private readonly LoginRequest _defaultLoginRequest = new()
    {
        Username = "username",
        Password = "password",
    };

    public AuthenticationControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthenticationService>();
                    services.AddSingleton(_mockedService);
                });
                builder.ConfigureNoDatabase();
                builder.UseSetting("AWS:LoadSecrets", $"{false}");
            })
            .CreateClient();
    }

    [Fact]
    public async Task Login_ShouldSetAccessTokenCookieOnSuccess()
    {
        var accessToken = "48b5becd-f98c-4897-98aa-be37eecb6a68";
        _mockedService
            .Login(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                LoginResult.Ok(new AuthenticationCredentialsDto() { AccessToken = accessToken })
            );

        var response = await _client.PostAsJsonAsync(
            new Uri("/auth/login", UriKind.Relative),
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        response.Headers.TryGetValues("Set-Cookie", out var cookies).ShouldBeTrue();

        var cookie = cookies.SingleOrDefault(x =>
            x.StartsWith($"access_token={accessToken}", StringComparison.InvariantCulture)
        );

        cookie.ShouldNotBeNull();
        cookie.ShouldContain($" expires=");
        cookie.ShouldContain(" secure");
        cookie.ShouldContain(" samesite=lax");
        cookie.ShouldContain(" httponly");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorisedOnUnauthorised()
    {
        _mockedService
            .Login(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(LoginResult.Err(new LoginError.Unauthorised()));

        var response = await _client.PostAsJsonAsync(
            new Uri("/auth/login", UriKind.Relative),
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var cookieExists =
            response.Headers.TryGetValues("Set-Cookie", out var cookies)
            && cookies.Any(x => x.StartsWith($"access_token=", StringComparison.InvariantCulture));

        cookieExists.ShouldBeFalse();
    }

    [Fact]
    public async Task Login_WhenEitherUsernameOrPasswordAreNotSet_ShouldReturnABadRequest()
    {
        Func<LoginRequest, LoginRequest>[] modifiers =
        [
            (x) => x with { Username = null! },
            (x) => x with { Username = string.Empty },
            (x) => x with { Password = null! },
            (x) => x with { Password = string.Empty },
        ];

        foreach (var modifier in modifiers)
        {
            var response = await _client.PostAsJsonAsync(
                new Uri("/auth/login", UriKind.Relative),
                modifier(_defaultLoginRequest),
                TestContext.Current.CancellationToken
            );

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }
}
