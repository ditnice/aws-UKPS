using System.Net;
using System.Net.Http.Headers;
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
using UpdatePasswordResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.UpdatePasswordError
>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class AuthenticationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string LoginUrl = "/auth/login";
    private const string UpdatePasswordUrl = "/auth/update-password";

    private readonly IAuthenticationService _mockedService =
        Substitute.For<IAuthenticationService>();
    private readonly HttpClient _client;
    private readonly LoginRequest _defaultLoginRequest = new()
    {
        Username = "username",
        Password = "password",
    };
    private readonly UpdatePasswordCommand _defaultUpdatePasswordCommand = new()
    {
        Username = "username",
        NewPassword = "new-password",
        AuthenticationSessionId = "authentication-session-id",
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
            new Uri(LoginUrl, UriKind.Relative),
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        AssertCookieExistsAndValidateCookie(response.Headers, accessToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorisedOnUnauthorised()
    {
        _mockedService
            .Login(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(LoginResult.Err(new LoginError.Unauthorised()));

        var response = await _client.PostAsJsonAsync(
            new Uri(LoginUrl, UriKind.Relative),
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        AssertCookieDoesNotExist(response.Headers);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorisedAndChallengeDetailsOnChalleng()
    {
        var challengeError = new LoginError.Challenge(
            UkpsChallengeType.NewPasswordRequired,
            "session-id"
        );
        _mockedService
            .Login(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(LoginResult.Err(challengeError));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            new Uri(LoginUrl, UriKind.Relative),
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        LoginError.Challenge? data = await response.Content.ReadFromJsonAsync<LoginError.Challenge>(
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );
        data.ShouldBe(challengeError);
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
                new Uri(LoginUrl, UriKind.Relative),
                modifier(_defaultLoginRequest),
                TestContext.Current.CancellationToken
            );

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            AssertCookieDoesNotExist(response.Headers);
        }
    }

    [Fact]
    public async Task UpdatePassword_OnSuccess_ShouldSetAccessToken()
    {
        var accessToken = "48b5becd-f98c-4897-98aa-be37eecb6a68";
        _mockedService
            .UpdatePassword(Arg.Any<UpdatePasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(
                UpdatePasswordResult.Ok(
                    new AuthenticationCredentialsDto() { AccessToken = accessToken }
                )
            );

        var response = await _client.PostAsJsonAsync(
            new Uri(UpdatePasswordUrl, UriKind.Relative),
            _defaultUpdatePasswordCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        AssertCookieExistsAndValidateCookie(response.Headers, accessToken);
    }

    [Fact]
    public async Task UpdatePassword_ShouldReturnUnauthorisedOnUnauthorised()
    {
        _mockedService
            .UpdatePassword(Arg.Any<UpdatePasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(UpdatePasswordResult.Err(new UpdatePasswordError.Unauthorised()));

        var response = await _client.PostAsJsonAsync(
            new Uri(UpdatePasswordUrl, UriKind.Relative),
            _defaultUpdatePasswordCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        AssertCookieDoesNotExist(response.Headers);
    }

    [Fact]
    public async Task UpdatePassword_WhenAnyPropertyIsNotSet_ShouldReturnBadRequest()
    {
        Func<UpdatePasswordCommand, UpdatePasswordCommand>[] modifiers =
        [
            (x) => x with { Username = null! },
            (x) => x with { Username = string.Empty },
            (x) => x with { NewPassword = null! },
            (x) => x with { NewPassword = string.Empty },
            (x) => x with { AuthenticationSessionId = null! },
            (x) => x with { AuthenticationSessionId = string.Empty },
        ];

        foreach (var modifier in modifiers)
        {
            var response = await _client.PostAsJsonAsync(
                new Uri(UpdatePasswordUrl, UriKind.Relative),
                modifier(_defaultUpdatePasswordCommand),
                TestContext.Current.CancellationToken
            );

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            AssertCookieDoesNotExist(response.Headers);
        }
    }

    private static void AssertCookieExistsAndValidateCookie(
        HttpResponseHeaders headers,
        string expectedAccessToken
    )
    {
        headers.TryGetValues("Set-Cookie", out var cookies).ShouldBeTrue();

        var cookie = cookies.SingleOrDefault(x =>
            x.StartsWith($"access_token={expectedAccessToken}", StringComparison.InvariantCulture)
        );

        cookie.ShouldNotBeNull();
        cookie.ShouldContain($" expires=");
        cookie.ShouldContain(" secure");
        cookie.ShouldContain(" samesite=lax");
        cookie.ShouldContain(" httponly");
    }

    private static void AssertCookieDoesNotExist(HttpResponseHeaders headers)
    {
        var cookieExists =
            headers.TryGetValues("Set-Cookie", out var cookies)
            && cookies.Any(x => x.StartsWith($"access_token=", StringComparison.InvariantCulture));

        cookieExists.ShouldBeFalse();
    }
}
