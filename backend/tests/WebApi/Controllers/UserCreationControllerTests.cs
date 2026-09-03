using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
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
    public async Task Post_WhenValuesNotValid_ShouldReturnHttpBadRequest()
    {
        (string Label, Func<OnboardUserCommandDto, OnboardUserCommandDto> Value)[] modifiers =
        [
            (
                nameof(OnboardUserCommandDto.NewUserEmail),
                x => x with { NewUserEmail = "invalid-email" }
            ),
            (
                nameof(OnboardUserCommandDto.NewUserEmail),
                x => x with { NewUserEmail = string.Empty }
            ),
            (
                nameof(OnboardUserCommandDto.ContactNumber),
                x => x with { ContactNumber = "invalid-contact-number" }
            ),
        ];

        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate();
        foreach (var modifier in modifiers)
        {
            var modifiedCommand = modifier.Value(command);
            HttpResponseMessage response = await _client.PostAsJsonAsync(
                OnboardEndpoint,
                modifiedCommand,
                TestContext.Current.CancellationToken
            );
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(
                TestContext.Current.CancellationToken
            );

            content.ShouldNotBeNull();
            content.Errors.ShouldContainKey(modifier.Label);
        }
    }
}
