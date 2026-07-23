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

namespace UKPS.Api.Tests.WebApi.Controllers;

public class UserCreationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string OnBoardEndpoint = "users/onboard";

    private readonly OnboardUserCommandDtoFaker _onboardUserCommandDtoFaker = new();
    private readonly HttpClient _client;
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
            })
            .CreateClient();
    }

    [Fact]
    public async Task Post_WhenValidRequest_ShouldCallOnBoardUserMethod()
    {
        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate();
        _ = await _client.PostAsJsonAsync(
            OnBoardEndpoint,
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
            OnBoardEndpoint,
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
            OnBoardEndpoint,
            command,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Post_WhenEmailNotSet_ShouldReturnHttpBadRequest()
    {
        OnboardUserCommandDto command = _onboardUserCommandDtoFaker.Generate() with
        {
            NewUserEmail = string.Empty,
        };
        var response = await _client.PostAsJsonAsync(
            OnBoardEndpoint,
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
            OnBoardEndpoint,
            command,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
