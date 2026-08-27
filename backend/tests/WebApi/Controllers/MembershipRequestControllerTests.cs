using System.Net;
using System.Net.Http.Json;
using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.InternalServices.Authentication;
using GetUserMembershipRequestResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.UserMembershipRequestDto,
    UKPS.Api.Application.Users.Errors.GetUserMembershipRequestError
>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class MembershipRequestControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int ExistingUserId = 1;
    private const int ExistingOrganisationId = 2;
    private readonly IMembershipRequestService _mock = Substitute.For<IMembershipRequestService>();
    private readonly HttpClient _client;
    private readonly UserMembershipRequestDto _userMembershipRequest;

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

        _userMembershipRequest = new UserMembershipRequestDtoFaker().Generate();
        _mock
            .GetUserMembershipRequest(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                GetUserMembershipRequestResult.Err(new GetUserMembershipRequestError.NotFound())
            );
        _mock
            .GetUserMembershipRequest(
                ExistingOrganisationId,
                ExistingUserId,
                Arg.Any<CancellationToken>()
            )
            .Returns(GetUserMembershipRequestResult.Ok(_userMembershipRequest));
        _mock
            .ApproveRequest(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<ApproveRequestError>.Err(new ApproveRequestError.RequestNotFound()));
        _mock
            .ApproveRequest(ExistingOrganisationId, ExistingUserId, Arg.Any<CancellationToken>())
            .Returns(Result<ApproveRequestError>.Ok());

        _mock
            .RejectRequest(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<RejectRequestError>.Err(new RejectRequestError.RequestNotFound()));
        _mock
            .RejectRequest(ExistingOrganisationId, ExistingUserId, Arg.Any<CancellationToken>())
            .Returns(Result<RejectRequestError>.Ok());
    }

    [Fact]
    public async Task GetUserMembershipRequest_OnValidRequest_ReturnsOkWithTheValue()
    {
        HttpResponseMessage response = await SendGetUserMembershipRequest(
            ExistingOrganisationId,
            ExistingUserId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        UserMembershipRequestDto? content =
            await response.Content.ReadFromJsonAsync<UserMembershipRequestDto>(
                TestJsonOptions.Default,
                TestContext.Current.CancellationToken
            );

        content.ShouldNotBeNull();
        content.ShouldBe(_userMembershipRequest);
    }

    [Fact]
    public async Task GetUserMembershipRequest_OnRequestNotFound_ReturnsNotFound()
    {
        HttpResponseMessage response = await SendGetUserMembershipRequest(999, 999);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserMembershipRequest_OnNotAllowed_ReturnsForbidden()
    {
        _mock
            .GetUserMembershipRequest(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                GetUserMembershipRequestResult.Err(new GetUserMembershipRequestError.NotAllowed())
            );
        HttpResponseMessage response = await SendGetUserMembershipRequest(
            ExistingOrganisationId,
            ExistingUserId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveRequest_OnValidRequest_ReturnsOk()
    {
        HttpResponseMessage response = await SendApproveRequest(
            ExistingOrganisationId,
            ExistingUserId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ApproveRequest_OnValidRequest_CallsServiceWithId()
    {
        _ = await SendApproveRequest(ExistingOrganisationId, ExistingUserId);
        await _mock
            .Received(1)
            .ApproveRequest(ExistingOrganisationId, ExistingUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveRequest_OnRequestNotFound_ReturnsNotFound()
    {
        HttpResponseMessage response = await SendApproveRequest(999, 999);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApproveRequest_OnNotAllowed_ReturnsForbidden()
    {
        _mock
            .ApproveRequest(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<ApproveRequestError>.Err(new ApproveRequestError.NotAllowed()));
        HttpResponseMessage response = await SendApproveRequest(
            ExistingOrganisationId,
            ExistingUserId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectRequest_OnValidRequest_ReturnsOk()
    {
        HttpResponseMessage response = await SendRejectRequest(
            ExistingOrganisationId,
            ExistingUserId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RejectRequest_OnValidRequest_CallsServiceWithId()
    {
        _ = await SendRejectRequest(ExistingOrganisationId, ExistingUserId);
        await _mock
            .Received(1)
            .RejectRequest(ExistingOrganisationId, ExistingUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectRequest_OnRequestNotFound_ReturnsNotFound()
    {
        HttpResponseMessage response = await SendRejectRequest(999, 999);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RejectRequest_OnNotAllowed_ReturnsForbidden()
    {
        _mock
            .RejectRequest(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<RejectRequestError>.Err(new RejectRequestError.NotAllowed()));
        HttpResponseMessage response = await SendRejectRequest(
            ExistingOrganisationId,
            ExistingUserId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private async Task<HttpResponseMessage> SendApproveRequest(int organisationId, int userId)
    {
        using var content = new StringContent(string.Empty);
        return await _client.PatchAsync(
            new Uri($"{CreateBasedUrl(organisationId, userId)}/approve", UriKind.Relative),
            content,
            TestContext.Current.CancellationToken
        );
    }

    private async Task<HttpResponseMessage> SendRejectRequest(int organisationId, int userId)
    {
        using var content = new StringContent(string.Empty);
        return await _client.PatchAsync(
            new Uri($"{CreateBasedUrl(organisationId, userId)}/reject", UriKind.Relative),
            content,
            TestContext.Current.CancellationToken
        );
    }

    private async Task<HttpResponseMessage> SendGetUserMembershipRequest(
        int organisationId,
        int userId
    )
    {
        return await _client.GetAsync(
            new Uri(CreateBasedUrl(organisationId, userId), UriKind.Relative),
            TestContext.Current.CancellationToken
        );
    }

    private static string CreateBasedUrl(int organisationId, int userId)
    {
        return $"/organisations/{organisationId}/users/{userId}/membership-requests";
    }

    private sealed class UserMembershipRequestDtoFaker : Faker<UserMembershipRequestDto>
    {
        public UserMembershipRequestDtoFaker()
        {
            RuleFor(x => x.Id, f => f.Random.Int(1));
        }
    }
}
