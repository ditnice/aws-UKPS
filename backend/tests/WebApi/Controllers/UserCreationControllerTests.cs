using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
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
using UKPS.Api.WebApi.Controllers;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class UserCreationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string OnboardEndpoint = "users/onboard";

    private readonly OnboardUserCommandDtoFaker _onboardUserCommandDtoFaker = new();
    private readonly HttpClient _client;
    private readonly UserCreationController _controller;
    private readonly IUserAdministrationService _mockService =
        Substitute.For<IUserAdministrationService>();

    public UserCreationControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _mockService
            .OnboardUser(Arg.Any<OnboardUserCommandDto>(), Arg.Any<CancellationToken>())
            .Returns(Result<OnboardUserError>.Ok());
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
                    $"{DevAuthenticationConfiguration.SectionName}:{nameof(DevAuthenticationConfiguration.IsEnabled)}",
                    $"{true}"
                );
            })
            .CreateClient();
        _controller = new UserCreationController(_mockService);
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
    public async Task Post_WhenNotAllowedError_ShouldReturnHttpForbid()
    {
        _mockService
            .OnboardUser(Arg.Any<OnboardUserCommandDto>(), Arg.Any<CancellationToken>())
            .Returns(Result<OnboardUserError>.Err(new OnboardUserError.NotAllowed()));
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
            .Returns(Result<OnboardUserError>.Err(new OnboardUserError.UsernameAlreadyExists()));
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
            .Returns(Result<OnboardUserError>.Err(new OnboardUserError.InvalidOrganisation()));
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
        RegisterUserDto request = RegisterUserDto();
        RegisterUserDetailsDto expected = RegisterUserDetailsDto();

        _mockService
            .RegisterUser(request, TestContext.Current.CancellationToken)
            .Returns(Result<RegisterUserDetailsDto, RegisterUserError>.Ok(expected));

        ActionResult<RegisterUserDetailsDto> result = await _controller.RegisterUser(
            request,
            TestContext.Current.CancellationToken
        );

        OkObjectResult ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task RegisterUser_FieldsMissing_ReturnsBadRequest()
    {
        RegisterUserDto request = RegisterUserDto();
        _mockService
            .RegisterUser(Arg.Any<RegisterUserDto>(), TestContext.Current.CancellationToken)
            .Returns(
                Result<RegisterUserDetailsDto, RegisterUserError>.Err(
                    new RegisterUserError.MissingFields()
                )
            );
        ActionResult<RegisterUserDetailsDto> result = await _controller.RegisterUser(
            request,
            TestContext.Current.CancellationToken
        );
        result
            .Result.ShouldBeOfType<BadRequestObjectResult>()
            .Value.ShouldBe("Some of the data required is missing.");
    }

    private static RegisterUserDto RegisterUserDto() =>
        new()
        {
            FullName = "Test1",
            PhoneNumber = "0123456789",
            WorkEmail = "user@example.com",
            Organisation = "Test2",
        };

    private static RegisterUserDetailsDto RegisterUserDetailsDto() =>
        new()
        {
            FullName = "Test1",
            PhoneNumber = "0123456789",
            WorkEmail = "user@example.com",
        };
}
