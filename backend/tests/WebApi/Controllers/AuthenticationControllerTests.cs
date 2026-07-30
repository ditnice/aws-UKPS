using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Application.AuthorisationAdministration;
using UKPS.Api.Application.Common;
using UKPS.Api.Tests.Utilities.Fixtures;
using LoginResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.LoginError
>;
using SetupUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.AuthorisationAdministration.MultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.AuthorisationAdministration.UserSetupError
>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class AuthenticationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string LoginUrl = "/auth/login";
    private const string ValidateSetupTokenUrl = "/auth/validate-setup-token";
    private const string SetupUserUrl = "/auth/setup-user";

    private readonly IAuthorisationAdministrationService _mockedAuthorisationService =
        Substitute.For<IAuthorisationAdministrationService>();
    private readonly Guid _defaultSetupToken = Guid.Parse("48b5becd-f98c-4897-98aa-be37eecb6a68");
    private readonly IAuthenticationService _mockedService =
        Substitute.For<IAuthenticationService>();
    private readonly HttpClient _client;
    private readonly LoginRequest _defaultLoginRequest = new()
    {
        Username = "username",
        Password = "password",
    };
    private readonly SetupUserCommand _defaultSetupUserCommand = new()
    {
        NewPassword = "password",
        SetupToken = Guid.Parse("48b5becd-f98c-4897-98aa-be37eecb6a68"),
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
                    services.RemoveAll<IAuthorisationAdministrationService>();
                    services.AddSingleton(_mockedService);
                    services.AddSingleton(_mockedAuthorisationService);
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
    public async Task Login_ShouldReturnUnauthorisedAndChallengeDetailsOnChallenge()
    {
        var challengeError = new LoginError.Challenge(
            UkpsChallengeType.MultiFactorAuthenticationRequired,
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
    public async Task ValidateSetupToken_ShouldReturnOkOnValidToken()
    {
        _mockedAuthorisationService
            .Validate(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<SetupTokenValidationError>.Ok());

        var response = await _client.GetAsync(
            new Uri($"{ValidateSetupTokenUrl}?setupToken={_defaultSetupToken}", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ValidateSetupToken_ShouldReturnUnauthorizedWhenTokenHasExpired()
    {
        _mockedAuthorisationService
            .Validate(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<SetupTokenValidationError>.Err(new SetupTokenValidationError.Expired())
            );

        var response = await _client.GetAsync(
            new Uri($"{ValidateSetupTokenUrl}?setupToken={_defaultSetupToken}", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidateSetupToken_ShouldReturnNotFoundWhenTokenDoesNotExist()
    {
        _mockedAuthorisationService
            .Validate(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<SetupTokenValidationError>.Err(new SetupTokenValidationError.DoesNotExist())
            );

        var response = await _client.GetAsync(
            new Uri($"{ValidateSetupTokenUrl}?setupToken={_defaultSetupToken}"),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ValidateSetupToken_ShouldReturnUnauthorizedWhenTokenHasBeenConsumed()
    {
        _mockedAuthorisationService
            .Validate(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<SetupTokenValidationError>.Err(new SetupTokenValidationError.Consumed())
            );

        var response = await _client.GetAsync(
            new Uri($"{ValidateSetupTokenUrl}?setupToken={_defaultSetupToken}", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidateSetupToken_WhenTokenIsMissing_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync(
            new Uri(ValidateSetupTokenUrl, UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetupUser_ShouldReturnOkOnSuccess()
    {
        var exampleResponse = new MultiFactorAuthenticationSetupDto()
        {
            OtpAuthUri = new("Example Response"),
        };
        _mockedAuthorisationService
            .SetupUser(Arg.Any<SetupUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(SetupUserResult.Ok(exampleResponse));

        var response = await _client.PostAsJsonAsync(
            new Uri(SetupUserUrl, UriKind.Relative),
            _defaultSetupUserCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseBody =
            await response.Content.ReadFromJsonAsync<MultiFactorAuthenticationSetupDto>(
                TestJsonOptions.Default,
                TestContext.Current.CancellationToken
            );

        responseBody.ShouldNotBeNull();
        responseBody.ShouldBeEquivalentTo(exampleResponse);
    }

    [Fact]
    public async Task SetupUser_ShouldReturnUnauthorizedWhenTokenHasBeenConsumed()
    {
        _mockedAuthorisationService
            .SetupUser(Arg.Any<SetupUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(SetupUserResult.Err(new UserSetupError.Consumed()));

        var response = await _client.PostAsJsonAsync(
            new Uri(SetupUserUrl, UriKind.Relative),
            _defaultSetupUserCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetupUser_ShouldReturnBadRequestWhenPasswordIsInvalid()
    {
        _mockedAuthorisationService
            .SetupUser(Arg.Any<SetupUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(SetupUserResult.Err(new UserSetupError.InvalidPassword()));

        var response = await _client.PostAsJsonAsync(
            new Uri(SetupUserUrl, UriKind.Relative),
            _defaultSetupUserCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetupUser_ShouldReturnUnauthorizedWhenTokenHasExpired()
    {
        _mockedAuthorisationService
            .SetupUser(Arg.Any<SetupUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(SetupUserResult.Err(new UserSetupError.Expired()));

        var response = await _client.PostAsJsonAsync(
            new Uri(SetupUserUrl, UriKind.Relative),
            _defaultSetupUserCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetupUser_ShouldReturnNotFoundWhenTokenDoesNotExist()
    {
        _mockedAuthorisationService
            .SetupUser(Arg.Any<SetupUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(SetupUserResult.Err(new UserSetupError.DoesNotExist()));

        var response = await _client.PostAsJsonAsync(
            new Uri(SetupUserUrl, UriKind.Relative),
            _defaultSetupUserCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
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
        cookie.ShouldContain(" samesite=strict");
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
