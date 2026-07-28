using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Tests.Utilities.AssertionHelpers;

namespace UKPS.Api.Tests.Application.Authentication;

public sealed class AuthenticationServiceTests
{
    private const string ClientId = "client-id";
    private const string UserPoolId = "user-pool-id";
    private readonly IAmazonCognitoIdentityProvider _cognito =
        Substitute.For<IAmazonCognitoIdentityProvider>();
    private readonly IOptions<CognitoConfiguration> _options = Substitute.For<
        IOptions<CognitoConfiguration>
    >();

    private readonly AuthenticationService _sut;
    private readonly LoginRequest _request = new LoginRequest
    {
        Username = "user@example.com",
        Password = "password",
    };

    public AuthenticationServiceTests()
    {
        _options.Value.Returns(
            new CognitoConfiguration()
            {
                UserPoolId = UserPoolId,
                ClientId = ClientId,
                ClientSecret = "client-secret",
                Region = "eu-west-2",
            }
        );

        _sut = new AuthenticationService(_cognito, _options);
    }

    [Fact]
    public async Task Login_ShouldReturnCredentials_WhenAuthenticationSucceeds()
    {
        var accessToken = "access-token";

        _cognito
            .AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new AdminInitiateAuthResponse
                {
                    AuthenticationResult = new AuthenticationResultType
                    {
                        AccessToken = accessToken,
                    },
                }
            );

        var result = await _sut.Login(_request, CancellationToken.None);

        var value = result.ShouldBeSuccess();
        value.AccessToken.ShouldBe(accessToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorised_WhenAuthenticationResultIsNull()
    {
        _cognito
            .AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new AdminInitiateAuthResponse());

        var result = await _sut.Login(_request, CancellationToken.None);

        result.ShouldBeError().ShouldBeOfType<LoginError.Unauthorised>();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorised_WhenCognitoThrowsNotAuthorizedException()
    {
        _cognito
            .AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns<Task<AdminInitiateAuthResponse>>(_ =>
                throw new NotAuthorizedException("Invalid credentials")
            );

        var result = await _sut.Login(_request, CancellationToken.None);

        result.ShouldBeError().ShouldBeOfType<LoginError.Unauthorised>();
    }

    [Fact]
    public async Task Login_ShouldReturnNewPasswordRequiredErrorOnNewPasswordRequiredError()
    {
        _cognito
            .AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new AdminInitiateAuthResponse()
                {
                    ChallengeName = ChallengeNameType.SOFTWARE_TOKEN_MFA,
                }
            );

        var result = await _sut.Login(_request, CancellationToken.None);

        var error = result.ShouldBeError().ShouldBeOfType<LoginError.Challenge>();
        error.Type.ShouldBe(UkpsChallengeType.MultiFactorAuthenticationRequired);
    }

    [Fact]
    public async Task Login_ShouldThrowNotSupportedExceptionIfChallengeNotRecognised()
    {
        _cognito
            .AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new AdminInitiateAuthResponse()
                {
                    ChallengeName = new ChallengeNameType("Not Mapped Challenge") { },
                }
            );

        await Should.ThrowAsync<NotSupportedException>(() =>
            _sut.Login(_request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Login_ShouldNotCapture_WhenCognitoThrowsNoneNotAuthorizedException()
    {
        _cognito
            .AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns<Task<AdminInitiateAuthResponse>>(_ =>
                throw new InvalidOperationException("Other Exception")
            );

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _sut.Login(_request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Login_ShouldThrowArgumentException_WhenLoginRequestIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _sut.Login(null!, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Login_ShouldSendCorrectRequestToCognito()
    {
        _cognito
            .AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new AdminInitiateAuthResponse
                {
                    AuthenticationResult = new AuthenticationResultType { AccessToken = "token" },
                }
            );

        await _sut.Login(_request, CancellationToken.None);

        await _cognito
            .Received(1)
            .AdminInitiateAuthAsync(
                Arg.Is<AdminInitiateAuthRequest>(r =>
                    r.ClientId == ClientId
                    && r.UserPoolId == UserPoolId
                    && r.AuthFlow == AuthFlowType.ADMIN_USER_PASSWORD_AUTH
                    && r.AuthParameters["USERNAME"] == _request.Username
                    && r.AuthParameters["PASSWORD"] == _request.Password
                ),
                CancellationToken.None
            );
    }
}
