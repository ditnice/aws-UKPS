using System.Net;
using System.Net.Http.Json;
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

namespace UKPS.Api.Tests.WebApi.Controllers;

public class UserRegistrationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int ExistingOrganisationId = 2;
    private Guid _existingRegistrationRequestId = Guid.NewGuid();
    private readonly IUserRegistrationService _mock = Substitute.For<IUserRegistrationService>();
    private readonly HttpClient _client;

    public UserRegistrationControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IUserRegistrationService>();
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
            .ApproveRequest(Arg.Any<int>(), Guid.NewGuid(), Arg.Any<CancellationToken>())
            .Returns(Result<ApproveRequestError>.Err(new ApproveRequestError.RequestNotFound()));
        _mock
            .ApproveRequest(
                ExistingOrganisationId,
                _existingRegistrationRequestId,
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<ApproveRequestError>.Ok());

        _mock
            .RejectRequest(Arg.Any<int>(), Guid.NewGuid(), Arg.Any<CancellationToken>())
            .Returns(Result<RejectRequestError>.Err(new RejectRequestError.RequestNotFound()));
        _mock
            .RejectRequest(
                ExistingOrganisationId,
                _existingRegistrationRequestId,
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<RejectRequestError>.Ok());
    }

    [Fact]
    public async Task RegisterUser_IsValid_ReturnsDto()
    {
        var organisationId = 1;
        RegisterUserCommandDto request = RegisterUserCommandDto();
        RegisterUserConfirmationDto expected = RegisterUserConfirmationDto();

        _mock
            .RegisterUser(organisationId, request, Arg.Any<CancellationToken>())
            .Returns(Result<RegisterUserConfirmationDto, RegisterUserError>.Ok(expected));

        HttpResponseMessage response = await SendRegisterUserRequest(organisationId, request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<RegisterUserConfirmationDto>(
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );
        content.ShouldBe(expected);
    }

    [Fact]
    public async Task RegisterUser_FieldsMissing_ReturnsBadRequest()
    {
        RegisterUserCommandDto request = RegisterUserCommandDto();
        _mock
            .RegisterUser(
                Arg.Any<int>(),
                Arg.Any<RegisterUserCommandDto>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Result<RegisterUserConfirmationDto, RegisterUserError>.Err(
                    new RegisterUserError.MissingFields()
                )
            );
        HttpResponseMessage response = await SendRegisterUserRequest(1, request);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterUser_OrganisationNotFound_ReturnsNotFound()
    {
        RegisterUserCommandDto request = RegisterUserCommandDto();

        _mock
            .RegisterUser(
                Arg.Any<int>(),
                Arg.Any<RegisterUserCommandDto>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Result<RegisterUserConfirmationDto, RegisterUserError>.Err(
                    new RegisterUserError.OrganisationNotFound()
                )
            );

        HttpResponseMessage response = await SendRegisterUserRequest(1, request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserRegistrationById_UserExists_ReturnsDto()
    {
        var orgId = 1;
        var registrationId = Guid.NewGuid();
        RegisterUserConfirmationDto expected = RegisterUserConfirmationDto();
        _mock
            .GetUserRegistrationById(orgId, registrationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegisterUserConfirmationDto, GetUserDetailsError>.Ok(expected));

        HttpResponseMessage response = await SendGetUserMembershipRequestById(
            orgId,
            registrationId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<RegisterUserConfirmationDto>(
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );
        content.ShouldBe(expected);
    }

    [Fact]
    public async Task GetUserRegistrationById_UserDoesNotExist_ReturnsNotFound()
    {
        Guid requestGuid = Guid.NewGuid();
        _mock
            .GetUserRegistrationById(Arg.Any<int>(), requestGuid, Arg.Any<CancellationToken>())
            .Returns(
                Result<RegisterUserConfirmationDto, GetUserDetailsError>.Err(
                    new GetUserDetailsError.IdNotFound(requestGuid)
                )
            );
        HttpResponseMessage response = await SendGetUserMembershipRequestById(1, requestGuid);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserRegistrationById_UserNotAuthorised_ReturnsForbidden()
    {
        Guid requestId = Guid.NewGuid();
        _mock
            .GetUserRegistrationById(Arg.Any<int>(), requestId, Arg.Any<CancellationToken>())
            .Returns(
                Result<RegisterUserConfirmationDto, GetUserDetailsError>.Err(
                    new GetUserDetailsError.UserNotAuthorised()
                )
            );
        HttpResponseMessage response = await SendGetUserMembershipRequestById(1, requestId);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveRequest_OnValidRequest_ReturnsOk()
    {
        HttpResponseMessage response = await SendApproveRequest(
            ExistingOrganisationId,
            _existingRegistrationRequestId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ApproveRequest_OnValidRequest_CallsServiceWithId()
    {
        _ = await SendApproveRequest(ExistingOrganisationId, _existingRegistrationRequestId);
        await _mock
            .Received(1)
            .ApproveRequest(
                ExistingOrganisationId,
                _existingRegistrationRequestId,
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task ApproveRequest_OnRequestNotFound_ReturnsNotFound()
    {
        HttpResponseMessage response = await SendApproveRequest(999, Guid.NewGuid());
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApproveRequest_OnNotAllowed_ReturnsForbidden()
    {
        _mock
            .ApproveRequest(
                ExistingOrganisationId,
                _existingRegistrationRequestId,
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<ApproveRequestError>.Err(new ApproveRequestError.NotAllowed()));
        HttpResponseMessage response = await SendApproveRequest(
            ExistingOrganisationId,
            _existingRegistrationRequestId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveRequest_OnBadRequestResult_ReturnsBadRequest()
    {
        ApproveRequestError[] badRequestResults =
        [
            new ApproveRequestError.InvalidOrganisation(),
            new ApproveRequestError.UserAlreadyExists(),
            new ApproveRequestError.RegistrationRejected(),
        ];
        foreach (var result in badRequestResults)
        {
            _mock
                .ApproveRequest(
                    ExistingOrganisationId,
                    _existingRegistrationRequestId,
                    Arg.Any<CancellationToken>()
                )
                .Returns(Result<ApproveRequestError>.Err(result));
            HttpResponseMessage response = await SendApproveRequest(
                ExistingOrganisationId,
                _existingRegistrationRequestId
            );
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task RejectRequest_OnValidRequest_ReturnsOk()
    {
        HttpResponseMessage response = await SendRejectRequest(
            ExistingOrganisationId,
            _existingRegistrationRequestId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RejectRequest_OnValidRequest_CallsServiceWithId()
    {
        _ = await SendRejectRequest(ExistingOrganisationId, _existingRegistrationRequestId);
        await _mock
            .Received(1)
            .RejectRequest(
                ExistingOrganisationId,
                _existingRegistrationRequestId,
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task RejectRequest_OnRequestNotFound_ReturnsNotFound()
    {
        HttpResponseMessage response = await SendRejectRequest(999, Guid.NewGuid());
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RejectRequest_OnNotAllowed_ReturnsForbidden()
    {
        _mock
            .RejectRequest(
                ExistingOrganisationId,
                _existingRegistrationRequestId,
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<RejectRequestError>.Err(new RejectRequestError.NotAllowed()));
        HttpResponseMessage response = await SendRejectRequest(
            ExistingOrganisationId,
            _existingRegistrationRequestId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectRequest_RegistrationAlreadyApproved_ReturnsBadRequest()
    {
        _mock
            .RejectRequest(
                ExistingOrganisationId,
                _existingRegistrationRequestId,
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<RejectRequestError>.Err(new RejectRequestError.RegistrationApproved()));
        HttpResponseMessage response = await SendRejectRequest(
            ExistingOrganisationId,
            _existingRegistrationRequestId
        );
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<HttpResponseMessage> SendRegisterUserRequest(
        int organisationId,
        RegisterUserCommandDto request
    )
    {
        return await _client.PostAsJsonAsync(
            new Uri($"organisations/{organisationId}/membership-requests", UriKind.Relative),
            request,
            TestContext.Current.CancellationToken
        );
    }

    private async Task<HttpResponseMessage> SendGetUserMembershipRequestById(
        int organisationId,
        Guid registrationId
    )
    {
        return await _client.GetAsync(
            new Uri(
                $"organisations/{organisationId}/membership-requests/{registrationId}",
                UriKind.Relative
            ),
            TestContext.Current.CancellationToken
        );
    }

    private static RegisterUserCommandDto RegisterUserCommandDto() =>
        new()
        {
            FullName = "Test1",
            PhoneNumber = "07845796823",
            WorkEmail = "user@example.com",
        };

    private static RegisterUserConfirmationDto RegisterUserConfirmationDto() =>
        new()
        {
            RequestGuid = Guid.NewGuid(),
            OrganisationName = "Test",
            FullName = "Test2",
            PhoneNumber = "07845796823",
            WorkEmail = "user@example.com",
        };

    private async Task<HttpResponseMessage> SendApproveRequest(int organisationId, Guid requestGuid)
    {
        using StringContent stringContent = new StringContent(string.Empty);
        var x = new Uri($"{CreateBasedUrl(organisationId, requestGuid)}/approve", UriKind.Relative);
        return await _client.PatchAsync(x, stringContent, TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> SendRejectRequest(int organisationId, Guid requestGuid)
    {
        using StringContent stringContent = new StringContent(string.Empty);
        return await _client.PatchAsync(
            new Uri($"{CreateBasedUrl(organisationId, requestGuid)}/reject", UriKind.Relative),
            stringContent,
            TestContext.Current.CancellationToken
        );
    }

    private static string CreateBasedUrl(int organisationId, Guid requestGuid)
    {
        return $"/organisations/{organisationId}/membership-requests/{requestGuid}";
    }
}
