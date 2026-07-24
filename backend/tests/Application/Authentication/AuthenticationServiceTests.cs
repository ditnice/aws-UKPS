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
    private readonly IAmazonCognitoIdentityProvider _cognito =
        Substitute.For<IAmazonCognitoIdentityProvider>();
    private readonly IOptions<CognitoConfiguration> _options = Substitute.For<
        IOptions<CognitoConfiguration>
    >();

    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _options.Value.Returns(
            new CognitoConfiguration
            {
                AccessKey = "access-key",
                Region = "region",
                SecretKey = "secret-key",
                ClientId = ClientId,
            }
        );

        _sut = new AuthenticationService(_cognito, _options);
    }

    [Fact]
    public async Task Login_ShouldReturnCredentials_WhenAuthenticationSucceeds()
    {
        var accessToken = "access-token";
        var request = new LoginRequest { Username = "user@example.com", Password = "password" };

        _cognito
            .InitiateAuthAsync(Arg.Any<InitiateAuthRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new InitiateAuthResponse
                {
                    AuthenticationResult = new AuthenticationResultType
                    {
                        AccessToken = accessToken,
                    },
                }
            );

        var result = await _sut.Login(request, CancellationToken.None);

        var value = result.ShouldBeSuccess();
        value.AccessToken.ShouldBe(accessToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorised_WhenAuthenticationResultIsNull()
    {
        var request = new LoginRequest { Username = "user@example.com", Password = "password" };

        _cognito
            .InitiateAuthAsync(Arg.Any<InitiateAuthRequest>(), Arg.Any<CancellationToken>())
            .Returns(new InitiateAuthResponse());

        var result = await _sut.Login(request, CancellationToken.None);

        result.ShouldBeError().ShouldBeOfType<LoginError.Unauthorised>();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorised_WhenCognitoThrowsNotAuthorizedException()
    {
        var request = new LoginRequest { Username = "user@example.com", Password = "password" };

        _cognito
            .InitiateAuthAsync(Arg.Any<InitiateAuthRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<InitiateAuthResponse>>(_ =>
                throw new NotAuthorizedException("Invalid credentials")
            );

        var result = await _sut.Login(request, CancellationToken.None);

        result.ShouldBeError().ShouldBeOfType<LoginError.Unauthorised>();
    }

    [Fact]
    public async Task Login_ShouldSendCorrectRequestToCognito()
    {
        var request = new LoginRequest { Username = "user@example.com", Password = "password" };

        _cognito
            .InitiateAuthAsync(Arg.Any<InitiateAuthRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new InitiateAuthResponse
                {
                    AuthenticationResult = new AuthenticationResultType { AccessToken = "token" },
                }
            );

        await _sut.Login(request, CancellationToken.None);

        await _cognito
            .Received(1)
            .InitiateAuthAsync(
                Arg.Is<InitiateAuthRequest>(r =>
                    r.ClientId == ClientId
                    && r.AuthFlow == AuthFlowType.USER_PASSWORD_AUTH
                    && r.AuthParameters["USERNAME"] == request.Username
                    && r.AuthParameters["PASSWORD"] == request.Password
                ),
                CancellationToken.None
            );
    }
}
