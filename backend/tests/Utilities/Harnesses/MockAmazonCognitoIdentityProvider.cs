using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using NSubstitute;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal sealed class MockAmazonCognitoIdentityProvider
{
    public string RefreshToken = "refresh-token";
    public string ValidMfaCode { get; } = "123456";
    public string ValidAuthenticationSession { get; } = "valid-auth-session";
    public string InvalidPassword { get; } = "invalid-password";
    public MockUser TestUser { get; } =
        new MockUser() { Username = "test-user", Password = "test-user-password-123" };

    public IReadOnlyCollection<MockUser> Users => _users;

    public IAmazonCognitoIdentityProvider Mock { get; init; } =
        Substitute.For<IAmazonCognitoIdentityProvider>();

    private List<MockUser> _users;

    private readonly HashSet<string> _mfaSessions = new(StringComparer.Ordinal);

    public MockAmazonCognitoIdentityProvider()
    {
        _users = [TestUser];

        Mock.WhenForAnyArgs(x => x.AdminCreateUserAsync(default!, default!))
            .Do(callInfo =>
            {
                var request = callInfo.Arg<AdminCreateUserRequest>();
                _users.Add(new() { Username = request.Username });
            });

        Mock.AdminSetUserPasswordAsync(
                Arg.Any<AdminSetUserPasswordRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<AdminSetUserPasswordRequest>();
                var password = request.Password;

                if (string.Equals(password, InvalidPassword, StringComparison.Ordinal))
                {
                    throw new InvalidPasswordException();
                }

                return Task.FromResult(new AdminSetUserPasswordResponse());
            });

        Mock.GetTokensFromRefreshTokenAsync(
                Arg.Any<GetTokensFromRefreshTokenRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<GetTokensFromRefreshTokenRequest>();

                if (IsValidRefreshToken(request.RefreshToken))
                {
                    return Task.FromResult(
                        new GetTokensFromRefreshTokenResponse
                        {
                            AuthenticationResult = new AuthenticationResultType
                            {
                                AccessToken = "access-token",
                                IdToken = "id-token",
                                RefreshToken = RefreshToken,
                            },
                        }
                    );
                }
                else
                {
                    throw new NotAuthorizedException("Invalid refresh token.");
                }
            });

        Mock.AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<AdminInitiateAuthRequest>();

                var username = request.AuthParameters["USERNAME"];

                var user = _users.SingleOrDefault(x =>
                    string.Equals(x.Username, username, StringComparison.Ordinal)
                );

                if (user is null)
                {
                    throw new UserNotFoundException($"User '{username}' not found.");
                }

                if (!user.MfaSetup)
                {
                    var session = Guid.NewGuid().ToString();
                    _mfaSessions.Add(session);

                    return Task.FromResult(
                        new AdminInitiateAuthResponse
                        {
                            ChallengeName = ChallengeNameType.MFA_SETUP,
                            Session = session,
                        }
                    );
                }

                return Task.FromResult(
                    new AdminInitiateAuthResponse
                    {
                        AuthenticationResult = new AuthenticationResultType
                        {
                            AccessToken = "access-token",
                            IdToken = "id-token",
                            RefreshToken = RefreshToken,
                        },
                    }
                );
            });

        Mock.AssociateSoftwareTokenAsync(
                Arg.Any<AssociateSoftwareTokenRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<AssociateSoftwareTokenRequest>();

                if (!IsSessionIdValid(request.Session))
                {
                    throw new NotAuthorizedException("Invalid authentication session.");
                }

                var newSession = Guid.NewGuid().ToString();
                _mfaSessions.Add(newSession);

                return Task.FromResult(
                    new AssociateSoftwareTokenResponse
                    {
                        SecretCode = "JBSWY3DPEHPK3PXP",
                        Session = newSession,
                    }
                );
            });

        Mock.VerifySoftwareTokenAsync(
                Arg.Any<VerifySoftwareTokenRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<VerifySoftwareTokenRequest>();

                if (!IsSessionIdValid(request.Session))
                {
                    throw new NotAuthorizedException("Invalid authentication session.");
                }

                if (!IsValidMfaToken(request.UserCode))
                {
                    throw new CodeMismatchException("Invalid MFA code.");
                }

                var newSession = Guid.NewGuid().ToString();
                _mfaSessions.Add(newSession);

                return Task.FromResult(
                    new VerifySoftwareTokenResponse
                    {
                        Status = VerifySoftwareTokenResponseType.SUCCESS,
                        Session = newSession,
                    }
                );
            });

        Mock.AdminRespondToAuthChallengeAsync(
                Arg.Any<AdminRespondToAuthChallengeRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<AdminRespondToAuthChallengeRequest>();

                if (!IsSessionIdValid(request.Session))
                {
                    throw new NotAuthorizedException("Invalid session.");
                }

                var username = request.ChallengeResponses["USERNAME"];
                var user = _users.SingleOrDefault(x =>
                    string.Equals(x.Username, username, StringComparison.Ordinal)
                );

                if (user is null)
                {
                    throw new UserNotFoundException($"User '{username}' not found.");
                }

                var mfaCode = request.ChallengeResponses["SOFTWARE_TOKEN_MFA_CODE"];
                if (!IsValidMfaToken(mfaCode))
                {
                    throw new CodeMismatchException("Invalid MFA code.");
                }

                _users = _users.Select(x => x == user ? x with { MfaSetup = true } : x).ToList();

                return Task.FromResult(
                    new AdminRespondToAuthChallengeResponse
                    {
                        AuthenticationResult = new AuthenticationResultType
                        {
                            AccessToken = "access-token",
                            IdToken = "id-token",
                            RefreshToken = RefreshToken,
                        },
                    }
                );
            });
    }

    private bool IsValidRefreshToken(string refreshToken)
    {
        return string.Equals(refreshToken, RefreshToken, StringComparison.Ordinal);
    }

    private bool IsValidMfaToken(string userCode)
    {
        return string.Equals(userCode, ValidMfaCode, StringComparison.Ordinal);
    }

    private bool IsSessionIdValid(string session)
    {
        return string.Equals(ValidAuthenticationSession, session, StringComparison.Ordinal)
            || _mfaSessions.Contains(session);
    }

    internal void AddCurrentUser(MockUser mockUser)
    {
        _users.Add(mockUser);
    }

    internal MockUser? GetUser(string targetUser)
    {
        return _users.FirstOrDefault(x =>
            string.Equals(x.Username, targetUser, StringComparison.Ordinal)
        );
    }
}
