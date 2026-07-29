using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using NSubstitute;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal sealed class MockAmazonCognitoIdentityProvider
{
    public IReadOnlyCollection<string> Users => _users;
    public IAmazonCognitoIdentityProvider Mock { get; init; } =
        Substitute.For<IAmazonCognitoIdentityProvider>();

    private readonly List<string> _users = new List<string>();

    public MockAmazonCognitoIdentityProvider()
    {
        Mock.WhenForAnyArgs(x => x.AdminCreateUserAsync(default!, default!))
            .Do(
                (callInfo) =>
                {
                    var request = callInfo.Arg<AdminCreateUserRequest>();
                    _users.Add(request.Username);
                }
            );
    }
}
