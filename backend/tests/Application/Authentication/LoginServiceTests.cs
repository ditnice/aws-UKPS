using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using LoginResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;

namespace UKPS.Api.Tests.Application.Authentication;

[Collection(DatabaseCollection.Name)]
public class LoginServiceTests : DatabaseTestBase
{
    private readonly IServiceTestHarness<ILoginService> _harness;

    private readonly DateTime _testTime = new DateTime(2022, 10, 11, 12, 14, 48, DateTimeKind.Utc);
    private LoginRequest _defaultLoginRequest = null!;
    private RespondToMultiFactorAuthenticationChallengeCommand _defaultResponseToMfaCommand = null!;
    private User _user = null!;

    public LoginServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _harness = new ServiceTestHarness<ILoginService>(Context)
            .UpdateCurrentTime(_testTime)
            .ConfigureServices(services =>
            {
                services.AddSingleton(
                    Options.Create(
                        new CognitoConfiguration
                        {
                            ClientId = "client-id",
                            Region = "eu-west-2",
                            ServiceUrlOverride = new Uri("https://cognito.example.com"),
                            ClientSecret = "client-secret",
                            UserPoolId = "user-pool-id",
                        }
                    )
                );
                return services;
            });
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        var testUser = _harness.Cognito.TestUser;
        var userFaker = new UserFaker().RuleFor(
            x => x.CognitoUsername,
            _ => new CognitoUsername() { Value = testUser.Username }
        );
        _user = await AddEntity(userFaker.Generate(), TestContext.Current.CancellationToken);
        _defaultLoginRequest = new LoginRequest()
        {
            Username = _user.WorkEmail,
            Password =
                testUser.Password
                ?? throw new InvalidOperationException("The test users password is not set."),
        };
        _defaultResponseToMfaCommand = new RespondToMultiFactorAuthenticationChallengeCommand()
        {
            Username = _user.WorkEmail,
            Code = _harness.Cognito.ValidMfaCode,
            AuthenticationSession = "authentication-session",
        };
    }

    [Fact]
    public async Task Login_ShouldRespondWithAChallenge()
    {
        LoginResult loginResult = await _harness.Service.Login(
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );
        loginResult.ShouldBeError().ShouldBeOfType<InitiateAuthenticationError.Challenge>();
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_WhenCodeAndSessionAreCorrect_ShouldReturnSuccessfulLogin()
    {
        InitiateAuthenticationError.Challenge challenge = await AttemptLoginAndGetChallenge();

        LoginResult result = await _harness.Service.RespondToMultiFactorAuthenticationChallenge(
            _defaultResponseToMfaCommand with
            {
                AuthenticationSession = challenge.AuthenticationSession,
            },
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_WhenCodeIsIncorrect_ShouldReturnError()
    {
        InitiateAuthenticationError.Challenge challenge = await AttemptLoginAndGetChallenge();
        LoginResult result = await _harness.Service.RespondToMultiFactorAuthenticationChallenge(
            _defaultResponseToMfaCommand with
            {
                AuthenticationSession = challenge.AuthenticationSession,
                Code = "incorrect-code",
            },
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<InitiateAuthenticationError.Unauthorised>();
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_SessionIsIncorrect_ShouldReturnError()
    {
        InitiateAuthenticationError.Challenge _ = await AttemptLoginAndGetChallenge();
        LoginResult result = await _harness.Service.RespondToMultiFactorAuthenticationChallenge(
            _defaultResponseToMfaCommand with
            {
                AuthenticationSession = "incorrect-session",
            },
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<InitiateAuthenticationError.Unauthorised>();
    }

    [Fact]
    public async Task RefreshAuthenticationToken_ShouldReturnRefreshedAuthenticationCredentials()
    {
        AuthenticationCredentialsDto credentials = await CompleteFullLogin();
        LoginResult result = await _harness.Service.RefreshAuthenticationToken(
            new RefreshAuthenticationTokenCommand() { RefreshToken = credentials.RefreshToken },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task RefreshAuthenticationToken_WhenRefreshTokenIsInvalid_ShouldReturnUnauthorisedResult()
    {
        AuthenticationCredentialsDto _ = await CompleteFullLogin();
        var result = await _harness.Service.RefreshAuthenticationToken(
            new RefreshAuthenticationTokenCommand() { RefreshToken = "invalid-token" },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<InitiateAuthenticationError.Unauthorised>();
    }

    private async Task<InitiateAuthenticationError.Challenge> AttemptLoginAndGetChallenge()
    {
        LoginResult loginResult = await _harness.Service.Login(
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );
        InitiateAuthenticationError.Challenge challenge = loginResult
            .ShouldBeError()
            .ShouldBeOfType<InitiateAuthenticationError.Challenge>();
        return challenge;
    }

    private async Task<AuthenticationCredentialsDto> CompleteFullLogin()
    {
        InitiateAuthenticationError.Challenge challenge = await AttemptLoginAndGetChallenge();

        LoginResult result = await _harness.Service.RespondToMultiFactorAuthenticationChallenge(
            _defaultResponseToMfaCommand with
            {
                AuthenticationSession = challenge.AuthenticationSession,
            },
            TestContext.Current.CancellationToken
        );
        AuthenticationCredentialsDto loginResult = result.ShouldBeSuccess();
        return loginResult;
    }
}
