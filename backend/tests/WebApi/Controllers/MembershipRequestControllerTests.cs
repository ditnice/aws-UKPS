using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class MembershipRequestControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int ExistingRequestId = 1;
    private const int NoneExistingUserId = 2;
    private readonly IMembershipRequestService _mock = Substitute.For<IMembershipRequestService>();
    private readonly HttpClient _client;

    public MembershipRequestControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IMembershipRequestService>();
                    services.AddSingleton(_mock);
                });
                builder.ConfigureNoDatabase();
                builder.UseSetting("AWS:LoadSecrets", $"{false}");
                builder.UseSetting(
                    $"{DevAuthenticationOptions.SectionName}:{nameof(DevAuthenticationOptions.IsEnabled)}",
                    $"{true}"
                );
            })
            .CreateClient();

        _mock
            .ApproveRequest(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<ApproveRequestError>.Err(new ApproveRequestError.RequestNotFound()));
        _mock
            .ApproveRequest(ExistingRequestId, Arg.Any<CancellationToken>())
            .Returns(Result<ApproveRequestError>.Ok());

        _mock
            .RejectRequest(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<RejectRequestError>.Err(new RejectRequestError.RequestNotFound()));
        _mock
            .RejectRequest(ExistingRequestId, Arg.Any<CancellationToken>())
            .Returns(Result<RejectRequestError>.Ok());
    }

    [Fact]
    public async Task ApproveRequest_OnValidRequest_ReturnsOk()
    {
        HttpResponseMessage response = await SendApproveRequest(ExistingRequestId);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ApproveRequest_OnValidRequest_CallsServiceWithId()
    {
        _ = await SendApproveRequest(ExistingRequestId);
        await _mock.Received(1).ApproveRequest(ExistingRequestId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveRequest_OnRequestNotFound_ReturnsNotFound()
    {
        HttpResponseMessage response = await SendApproveRequest(NoneExistingUserId);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveRequest_OnNotAllowed_ReturnsForbidden()
    {
        _mock
            .ApproveRequest(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<ApproveRequestError>.Err(new ApproveRequestError.NotAllowed()));
        HttpResponseMessage response = await SendApproveRequest(NoneExistingUserId);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private async Task<HttpResponseMessage> SendApproveRequest(int requestId)
    {
        using var content = new StringContent(string.Empty);
        return await _client.PatchAsync(
            new Uri($"/membership-requests/{requestId}", UriKind.Relative),
            content,
            TestContext.Current.CancellationToken
        );
    }
}
