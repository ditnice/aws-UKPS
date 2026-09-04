using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Tests.Application.Users;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.InternalServices.Authentication;
using UserOnboardingResult = UKPS.Api.Application.Common.Result<
    int,
    UKPS.Api.Application.Users.Errors.OnboardUserError
>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class UserCreationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string OnboardEndpoint = "users/onboard";

    private readonly OnboardUserCommandDtoFaker _onboardUserCommandDtoFaker = new();
    private readonly HttpClient _client;
    private readonly IUserAdministrationService _mockService =
        Substitute.For<IUserAdministrationService>();

    public UserCreationControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _mockService
            .OnboardUser(Arg.Any<OnboardUserCommandDto>(), Arg.Any<CancellationToken>())
            .Returns(UserOnboardingResult.Ok(1));
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IUserAdministrationService>();
                    services.AddSingleton(_mockService);
                });
                builder.ConfigureNoDatabase();
                builder.UseSetting("AWS:LoadSecrets", $"{false}");
                builder.UseSetting(
                    $"{DevAuthenticationOptions.SectionName}:{nameof(DevAuthenticationOptions.IsEnabled)}",
                    $"{true}"
                );
            })
            .CreateClient();
    }

    [Fact]
    public async Task Post_WhenValidRequest_ShouldCallOnboardUserMethod()
    {
        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate();
        _ = await _client.PostAsJsonAsync(
            OnboardEndpoint,
            command,
            TestContext.Current.CancellationToken
        );

        await _mockService.Received(1).OnboardUser(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Post_WhenSuccessResult_ShouldReturnHttpCreated()
    {
        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate();
        var response = await _client.PostAsJsonAsync(
            OnboardEndpoint,
            command,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_WhenSuccessResult_ShouldReturnTheNewUsersId()
    {
        const int newUserId = 42;
        _mockService
            .OnboardUser(Arg.Any<OnboardUserCommandDto>(), Arg.Any<CancellationToken>())
            .Returns(UserOnboardingResult.Ok(newUserId));
        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate();

        var response = await _client.PostAsJsonAsync(
            OnboardEndpoint,
            command,
            TestContext.Current.CancellationToken
        );

        OnboardedUserDto? body = await response.Content.ReadFromJsonAsync<OnboardedUserDto>(
            TestContext.Current.CancellationToken
        );
        body.ShouldNotBeNull();
        body.UserId.ShouldBe(newUserId);
    }

    [Fact]
    public async Task Post_WhenNotAllowedError_ShouldReturnHttpForbid()
    {
        _mockService
            .OnboardUser(Arg.Any<OnboardUserCommandDto>(), Arg.Any<CancellationToken>())
            .Returns(UserOnboardingResult.Err(new OnboardUserError.NotAllowed()));
        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate();
        var response = await _client.PostAsJsonAsync(
            OnboardEndpoint,
            command,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Post_WhenUserNameAlreadyExist_ShouldReturnConflict()
    {
        _mockService
            .OnboardUser(Arg.Any<OnboardUserCommandDto>(), Arg.Any<CancellationToken>())
            .Returns(UserOnboardingResult.Err(new OnboardUserError.UsernameAlreadyExists()));
        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate();
        var response = await _client.PostAsJsonAsync(
            OnboardEndpoint,
            command,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_WhenInvalidOrganisation_ShouldReturnBadRequest()
    {
        _mockService
            .OnboardUser(Arg.Any<OnboardUserCommandDto>(), Arg.Any<CancellationToken>())
            .Returns(UserOnboardingResult.Err(new OnboardUserError.InvalidOrganisation()));
        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate();
        var response = await _client.PostAsJsonAsync(
            OnboardEndpoint,
            command,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_WhenEmailNotSet_ShouldReturnHttpBadRequest()
    {
        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate() with
        {
            NewUserEmail = string.Empty,
        };
        var response = await _client.PostAsJsonAsync(
            OnboardEndpoint,
            command,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_WhenEmailNotValidaEmail_ShouldReturnHttpBadRequest()
    {
        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate() with
        {
            NewUserEmail = "invalid-email",
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            OnboardEndpoint,
            command,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterUser_IsValid_ReturnsDto()
    {
        RegisterUserCommandDto request = RegisterUserCommandDto();
        RegisterUserConfirmationDto expected = RegisterUserConfirmationDto();

        _mockService
            .RegisterUser(request, Arg.Any<CancellationToken>())
            .Returns(Result<RegisterUserConfirmationDto, RegisterUserError>.Ok(expected));

        var response = await _client.PostAsJsonAsync(
            new Uri("/users/register"),
            request,
            TestContext.Current.CancellationToken
        );
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
        _mockService
            .RegisterUser(Arg.Any<RegisterUserCommandDto>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<RegisterUserConfirmationDto, RegisterUserError>.Err(
                    new RegisterUserError.MissingFields()
                )
            );
        var response = await _client.PostAsJsonAsync(
            new Uri("/users/register"),
            request,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserRegistrationById_UserExists_ReturnsDto()
    {
        RegisterUserConfirmationDto expected = RegisterUserConfirmationDto();
        _mockService
            .GetUserRegistrationById(1, Arg.Any<CancellationToken>())
            .Returns(Result<RegisterUserConfirmationDto, GetUserDetailsError>.Ok(expected));

        var response = await _client.GetAsync(
            new Uri("/users/registration-requests/1"),
            TestContext.Current.CancellationToken
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
        _mockService
            .GetUserRegistrationById(1, Arg.Any<CancellationToken>())
            .Returns(
                Result<RegisterUserConfirmationDto, GetUserDetailsError>.Err(
                    new GetUserDetailsError.IdNotFound(1)
                )
            );
        var response = await _client.GetAsync(
            new Uri("/users/registration-request/1"),
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static RegisterUserCommandDto RegisterUserCommandDto() =>
        new()
        {
            FullName = "Test1",
            PhoneNumber = "07845796823",
            WorkEmail = "user@example.com",
            OrganisationId = 1,
        };

    private static RegisterUserConfirmationDto RegisterUserConfirmationDto() =>
        new()
        {
            Id = 1,
            OrganisationName = "Test",
            FullName = "Test2",
            PhoneNumber = "07845796823",
            WorkEmail = "user@example.com",
        };
}
