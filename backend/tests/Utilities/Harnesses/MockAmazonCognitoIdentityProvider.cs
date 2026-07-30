using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using NSubstitute;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal sealed class MockAmazonCognitoIdentityProvider
{
    public IReadOnlyCollection<MockUser> Users => _users;

    public IAmazonCognitoIdentityProvider Mock { get; init; } =
        Substitute.For<IAmazonCognitoIdentityProvider>();

    private readonly List<MockUser> _users = new();

    private readonly HashSet<string> _mfaSessions = new(StringComparer.Ordinal);

    public MockAmazonCognitoIdentityProvider()
    {
        Mock.WhenForAnyArgs(x => x.AdminCreateUserAsync(default!, default!))
            .Do(callInfo =>
            {
                var request = callInfo.Arg<AdminCreateUserRequest>();
                _users.Add(new() { Username = request.Username });
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
                            RefreshToken = "refresh-token",
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

                if (!_mfaSessions.Contains(request.Session))
                {
                    throw new InvalidParameterException("Invalid authentication session.");
                }

                var newSession = Guid.NewGuid().ToString();

                return Task.FromResult(
                    new AssociateSoftwareTokenResponse
                    {
                        SecretCode = "JBSWY3DPEHPK3PXP",
                        Session = newSession,
                    }
                );
            });
    }

    internal void AddCurrentUser(MockUser mockUser)
    {
        _users.Add(mockUser);
    }
}
