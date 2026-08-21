using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Bogus;
using NSubstitute;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal sealed class MockAmazonCognitoIdentityProvider
{
    public string RefreshToken = "refresh-token";
    public string ValidMfaCode { get; } = "123456";
    public string ValidAuthenticationSession { get; } = "valid-auth-session";
    public string InvalidPassword { get; } = "invalid-password";
    public MockUser TestUser { get; } =
        new MockUser()
        {
            Username = "test-user",
            Password = "test-user-password-123",
            IdentityId = "b7f3c5f9-8b2d-4a71-9e6c-3d4f0a8c1e52",
        };

    public IReadOnlyCollection<MockUser> Users => _users;

    public IAmazonCognitoIdentityProvider Mock { get; init; } =
        Substitute.For<IAmazonCognitoIdentityProvider>();

    private List<MockUser> _users;

    private readonly HashSet<string> _mfaSessions = new(StringComparer.Ordinal);

    public MockAmazonCognitoIdentityProvider()
    {
        _users = [TestUser];

        Mock.AdminCreateUserAsync(Arg.Any<AdminCreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                (callInfo) =>
                {
                    var request = callInfo.Arg<AdminCreateUserRequest>();
                    var identityId = Guid.NewGuid().ToString();
                    _users.Add(new() { Username = request.Username, IdentityId = identityId });
                    return new AdminCreateUserResponse()
                    {
                        User = new UserType()
                        {
                            Attributes = [new() { Name = "sub", Value = identityId }],
                        },
                    };
                }
            );

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

        Mock.AdminUpdateUserAttributesAsync(
                Arg.Any<AdminUpdateUserAttributesRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<AdminUpdateUserAttributesRequest>();
                var newUserName = request
                    .UserAttributes.FirstOrDefault(x =>
                        string.Equals(x.Name, "email", StringComparison.Ordinal)
                    )
                    ?.Value;
                _users = _users
                    .Select(u =>
                        string.Equals(u.Username, request.Username, StringComparison.Ordinal)
                            ? u with
                            {
                                Username = newUserName ?? u.Username,
                            }
                            : u
                    )
                    .ToList();
                return Task.FromResult(new AdminUpdateUserAttributesResponse());
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

    public sealed class MockUserFaker : Faker<MockUser>
    {
        public MockUserFaker()
        {
            RuleFor(x => x.Username, f => f.Internet.UserName());
            RuleFor(x => x.IdentityId, f => f.Random.Guid().ToString());
        }
    }
}
