using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Tests.Utilities.Fixtures;
using InitiatedAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;
using SetupUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.MultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.Authentication.Errors.UserSetupError
>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class AuthenticationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AccessTokenCookieName = "access_token";
    private const string RefreshTokenCookieName = "refresh_token";
    private const string CsrfCookieName = "csrf_token";

    private const string LoginUrl = "/auth/login";
    private const string RespondToMultiFactorAuthenticationChallengeUrl = "/auth/mfa";
    private const string RefreshUrl = "/auth/refresh";
    private const string ValidateSetupTokenUrl = "/auth/validate-setup-token";
    private const string SetupUserUrl = "/auth/setup-user";
    private const string VerifyMultiFactorAuthenticationUrl = "/auth/verify-mfa";

    private readonly IIdentityAdministrationService _mockedAuthorisationService =
        Substitute.For<IIdentityAdministrationService>();
    private readonly ILoginService _mockedLoginService = Substitute.For<ILoginService>();
    private readonly Guid _defaultSetupToken = Guid.Parse("48b5becd-f98c-4897-98aa-be37eecb6a68");
    private readonly HttpClient _client;
    private readonly LoginRequest _defaultLoginRequest = new()
    {
        Username = "username",
        Password = "password",
    };
    private readonly RespondToMultiFactorAuthenticationChallengeCommand _respondToMultiFactorAuthenticationChallengeCommand =
        new()
        {
            Username = "username",
            Code = "code",
            AuthenticationSession = "authentication-session",
        };
    private readonly SetupUserCommand _defaultSetupUserCommand = new()
    {
        NewPassword = "password",
        SetupToken = Guid.Parse("48b5becd-f98c-4897-98aa-be37eecb6a68"),
    };
    private readonly VerifyMultiFactorAuthenticationCommand _defaultVerifyMultiFactorAuthenticationCommand =
        new()
        {
            SetupToken = Guid.Parse("48b5becd-f98c-4897-98aa-be37eecb6a68"),
            Code = "234523",
            AuthenticationSession = "8743qfu34_gcfp3984fcn)3o4h34c98f349c_8h34",
        };
    private readonly AuthenticationCredentialsDto _validAuthenticationCredentials = new()
    {
        AccessToken = "access-token",
        RefreshToken = "refresh-token",
    };

    public AuthenticationControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ILoginService>();
                    services.RemoveAll<IIdentityAdministrationService>();
                    services.AddSingleton(_mockedLoginService);
                    services.AddSingleton(_mockedAuthorisationService);
                });
                builder.ConfigureNoDatabase();
                builder.UseSetting("AWS:LoadSecrets", $"{false}");
            })
            .CreateClient();

        _mockedLoginService
            .RefreshAuthenticationToken(
                Arg.Any<RefreshAuthenticationTokenCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(InitiatedAuthenticationResult.Ok(_validAuthenticationCredentials));
    }

    [Fact]
    public async Task Login_ShouldSetAccessTokenCookieOnSuccess()
    {
        _mockedLoginService
            .Login(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(InitiatedAuthenticationResult.Ok(_validAuthenticationCredentials));

        var response = await _client.PostAsJsonAsync(
            new Uri(LoginUrl, UriKind.Relative),
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        AssertCookieExistsAndValidateCookie(
            AccessTokenCookieName,
            response.Headers,
            _validAuthenticationCredentials.AccessToken
        );
        AssertCookieExistsAndValidateCookie(
            RefreshTokenCookieName,
            response.Headers,
            _validAuthenticationCredentials.RefreshToken
        );
        CookieExists(CsrfCookieName, response.Headers)
            .ShouldBeTrue($"The cookie [{CsrfCookieName}] should exist in the response headers.");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorisedOnUnauthorised()
    {
        _mockedLoginService
            .Login(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                InitiatedAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised())
            );

        var response = await _client.PostAsJsonAsync(
            new Uri(LoginUrl, UriKind.Relative),
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        AssertCookiesDoNotExist(response.Headers);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorisedAndChallengeDetailsOnChallenge()
    {
        var challengeError = new InitiateAuthenticationError.Challenge(
            UkpsChallengeType.MultiFactorAuthenticationRequired,
            "session-id"
        );
        _mockedLoginService
            .Login(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(InitiatedAuthenticationResult.Err(challengeError));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            new Uri(LoginUrl, UriKind.Relative),
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        InitiateAuthenticationError.Challenge? data =
            await response.Content.ReadFromJsonAsync<InitiateAuthenticationError.Challenge>(
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
            AssertCookiesDoNotExist(response.Headers);
        }
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_ShouldSetAccessTokenCookieOnSuccess()
    {
        _mockedLoginService
            .RespondToMultiFactorAuthenticationChallenge(
                Arg.Any<RespondToMultiFactorAuthenticationChallengeCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(InitiatedAuthenticationResult.Ok(_validAuthenticationCredentials));

        var response = await _client.PostAsJsonAsync(
            new Uri(RespondToMultiFactorAuthenticationChallengeUrl, UriKind.Relative),
            _respondToMultiFactorAuthenticationChallengeCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        AssertCookieExistsAndValidateCookie(
            AccessTokenCookieName,
            response.Headers,
            _validAuthenticationCredentials.AccessToken
        );
        AssertCookieExistsAndValidateCookie(
            RefreshTokenCookieName,
            response.Headers,
            _validAuthenticationCredentials.RefreshToken
        );
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_ShouldReturnUnauthorisedOnUnauthorised()
    {
        _mockedLoginService
            .RespondToMultiFactorAuthenticationChallenge(
                Arg.Any<RespondToMultiFactorAuthenticationChallengeCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                InitiatedAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised())
            );

        var response = await _client.PostAsJsonAsync(
            new Uri(RespondToMultiFactorAuthenticationChallengeUrl, UriKind.Relative),
            _respondToMultiFactorAuthenticationChallengeCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        AssertCookiesDoNotExist(response.Headers);
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_ShouldReturnUnauthorisedAndChallengeDetailsOnChallenge()
    {
        var challengeError = new InitiateAuthenticationError.Challenge(
            UkpsChallengeType.MultiFactorAuthenticationRequired,
            "session-id"
        );
        _mockedLoginService
            .RespondToMultiFactorAuthenticationChallenge(
                Arg.Any<RespondToMultiFactorAuthenticationChallengeCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(InitiatedAuthenticationResult.Err(challengeError));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            new Uri(RespondToMultiFactorAuthenticationChallengeUrl, UriKind.Relative),
            _respondToMultiFactorAuthenticationChallengeCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        InitiateAuthenticationError.Challenge? data =
            await response.Content.ReadFromJsonAsync<InitiateAuthenticationError.Challenge>(
                TestJsonOptions.Default,
                TestContext.Current.CancellationToken
            );
        data.ShouldBe(challengeError);
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_WhenInvalidParametersProvided_ShouldReturnABadRequest()
    {
        Func<
            RespondToMultiFactorAuthenticationChallengeCommand,
            RespondToMultiFactorAuthenticationChallengeCommand
        >[] modifiers =
        [
            (x) => x with { Username = null! },
            (x) => x with { Username = string.Empty },
            (x) => x with { Code = null! },
            (x) => x with { Code = string.Empty },
            (x) => x with { AuthenticationSession = null! },
            (x) => x with { AuthenticationSession = string.Empty },
        ];

        foreach (var modifier in modifiers)
        {
            var response = await _client.PostAsJsonAsync(
                new Uri(RespondToMultiFactorAuthenticationChallengeUrl, UriKind.Relative),
                modifier(_respondToMultiFactorAuthenticationChallengeCommand),
                TestContext.Current.CancellationToken
            );

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            AssertCookiesDoNotExist(response.Headers);
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
            new Uri($"{ValidateSetupTokenUrl}?setupToken={_defaultSetupToken}", UriKind.Relative),
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
        var exampleResponse = new MultiFactorAuthenticationSetupDtoFaker().Generate();
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

    [Fact]
    public async Task SetupUser_ShouldReturnUnauthorisedResultWhenNotAuthorised()
    {
        _mockedAuthorisationService
            .SetupUser(Arg.Any<SetupUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(SetupUserResult.Err(new UserSetupError.Unauthorised()));

        var response = await _client.PostAsJsonAsync(
            new Uri(SetupUserUrl, UriKind.Relative),
            _defaultSetupUserCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task VerifyMultiFactorAuthentication_ShouldReturnOkOnValidRequest()
    {
        _mockedAuthorisationService
            .VerifyMultiFactorAuthentication(
                _defaultVerifyMultiFactorAuthenticationCommand,
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<VerifyMultiFactorAuthenticationError>.Ok());

        var response = await _client.PostAsJsonAsync(
            new Uri(VerifyMultiFactorAuthenticationUrl, UriKind.Relative),
            _defaultVerifyMultiFactorAuthenticationCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task VerifyMultiFactorAuthentication_ShouldReturnBadRequestOnInvalidCode()
    {
        _mockedAuthorisationService
            .VerifyMultiFactorAuthentication(
                _defaultVerifyMultiFactorAuthenticationCommand,
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Result<VerifyMultiFactorAuthenticationError>.Err(
                    new VerifyMultiFactorAuthenticationError.InvalidCode()
                )
            );

        var response = await _client.PostAsJsonAsync(
            new Uri(VerifyMultiFactorAuthenticationUrl, UriKind.Relative),
            _defaultVerifyMultiFactorAuthenticationCommand,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyMultiFactorAuthentication_ShouldReturnPassDataToTheService()
    {
        _mockedAuthorisationService
            .VerifyMultiFactorAuthentication(
                _defaultVerifyMultiFactorAuthenticationCommand,
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<VerifyMultiFactorAuthenticationError>.Ok());

        var _ = await _client.PostAsJsonAsync(
            new Uri(VerifyMultiFactorAuthenticationUrl, UriKind.Relative),
            _defaultVerifyMultiFactorAuthenticationCommand,
            TestContext.Current.CancellationToken
        );

        await _mockedAuthorisationService
            .Received(1)
            .VerifyMultiFactorAuthentication(
                _defaultVerifyMultiFactorAuthenticationCommand,
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task VerifyMultiFactorAuthentication_WhenParametersAreMissing_ShouldReturnBadRequest()
    {
        _mockedAuthorisationService
            .VerifyMultiFactorAuthentication(
                _defaultVerifyMultiFactorAuthenticationCommand,
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<VerifyMultiFactorAuthenticationError>.Ok());

        Func<
            VerifyMultiFactorAuthenticationCommand,
            VerifyMultiFactorAuthenticationCommand
        >[] modifiers =
        [
            x => x with { Code = string.Empty },
            x => x with { AuthenticationSession = string.Empty },
        ];

        foreach (var modifier in modifiers)
        {
            var response = await _client.PostAsJsonAsync(
                new Uri(VerifyMultiFactorAuthenticationUrl, UriKind.Relative),
                modifier(_defaultVerifyMultiFactorAuthenticationCommand),
                TestContext.Current.CancellationToken
            );
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task Refresh_ShouldUpdateCookiesAndReturnOkResponse()
    {
        var response = await SendRefreshRequest();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        AssertCookieExistsAndValidateCookie(
            AccessTokenCookieName,
            response.Headers,
            _validAuthenticationCredentials.AccessToken
        );
        AssertCookieExistsAndValidateCookie(
            RefreshTokenCookieName,
            response.Headers,
            _validAuthenticationCredentials.RefreshToken
        );
        CookieExists(CsrfCookieName, response.Headers)
            .ShouldBeTrue($"The cookie [{CsrfCookieName}] should exist in the response headers.");
    }

    [Fact]
    public async Task Refresh_WhenTheRequestIsUnauthorised_ShouldReturnUnauthorisedResponse()
    {
        _mockedLoginService
            .RefreshAuthenticationToken(
                Arg.Any<RefreshAuthenticationTokenCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                InitiatedAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised())
            );
        var response = await SendRefreshRequest();

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        AssertCookiesDoNotExist(response.Headers);
    }

    [Fact]
    public async Task Refresh_WhenTheRequestIsChallenged_ShouldReturnUnauthorisedResponse()
    {
        _mockedLoginService
            .RefreshAuthenticationToken(
                Arg.Any<RefreshAuthenticationTokenCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                InitiatedAuthenticationResult.Err(
                    new InitiateAuthenticationError.Challenge(
                        UkpsChallengeType.MultiFactorAuthenticationRequired,
                        "session-id"
                    )
                )
            );
        var response = await SendRefreshRequest();

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        AssertCookiesDoNotExist(response.Headers);
    }

    [Fact]
    public async Task Refresh_WhenCsrfCookieNotSet_ShouldReturnUnauthorised()
    {
        var response = await SendRefreshRequest(csrfCookie: null);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        AssertCookiesDoNotExist(response.Headers);
    }

    [Fact]
    public async Task Refresh_WhenCsrfHeaderNotSet_ShouldReturnUnauthorised()
    {
        var response = await SendRefreshRequest(csrfHeader: null);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        AssertCookiesDoNotExist(response.Headers);
    }

    [Fact]
    public async Task Refresh_WhenCsrfCookieAndHeaderDoNotMatchNot_ShouldReturnUnauthorised()
    {
        var response = await SendRefreshRequest(csrfHeader: "versionA", csrfCookie: "versionB");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        AssertCookiesDoNotExist(response.Headers);
    }

    private async Task<HttpResponseMessage> SendRefreshRequest(
        string? csrfCookie = "test-csrf-token",
        string? csrfHeader = "test-csrf-token"
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(RefreshUrl, UriKind.Relative)
        );

        if (csrfHeader is not null)
        {
            request.Headers.Add("X-CSRF-Token", csrfHeader);
        }
        if (csrfCookie is not null)
        {
            request.Headers.Add("Cookie", $"{CsrfCookieName}={csrfCookie}");
        }

        return await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static void AssertCookieExistsAndValidateCookie(
        string cookieName,
        HttpResponseHeaders headers,
        string expectedAccessToken
    )
    {
        headers.TryGetValues("Set-Cookie", out var cookies).ShouldBeTrue();

        var cookie = cookies.SingleOrDefault(x =>
            x.StartsWith($"{cookieName}={expectedAccessToken}", StringComparison.InvariantCulture)
        );

        cookie.ShouldNotBeNull();
        cookie.ShouldContain($" expires=");
        cookie.ShouldContain(" secure");
        cookie.ShouldContain(" samesite=strict");
        cookie.ShouldContain(" httponly");
    }

    private static void AssertCookiesDoNotExist(HttpResponseHeaders headers)
    {
        string[] cookiesNames = [AccessTokenCookieName, RefreshTokenCookieName];
        foreach (var cookie in cookiesNames)
        {
            bool cookieExists = CookieExists(cookie, headers);
            cookieExists.ShouldBeFalse(
                $"The cookie [{cookie}] should not exist in the response headers."
            );
        }
    }

    private static bool CookieExists(string cookie, HttpResponseHeaders headers)
    {
        return headers.TryGetValues("Set-Cookie", out var cookies)
            && cookies.Any(x => x.StartsWith($"{cookie}=", StringComparison.InvariantCulture));
    }

    private sealed class MultiFactorAuthenticationSetupDtoFaker
        : Faker<MultiFactorAuthenticationSetupDto>
    {
        public MultiFactorAuthenticationSetupDtoFaker()
        {
            UseSeed(66);
            RuleFor(
                x => x.OtpAuthUri,
                f => new Uri(
                    $"otpauth://totp/example:{f.Internet.Email()}?secret={f.Random.AlphaNumeric(32)}&issuer=Example"
                )
            );

            RuleFor(x => x.AuthenticationSession, f => f.Random.Guid().ToString());
        }
    }
}
