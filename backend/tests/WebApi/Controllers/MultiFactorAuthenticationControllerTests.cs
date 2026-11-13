using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.MockInternalServices;
using UKPS.Api.WebApi.InternalServices.Authentication;
using InitiatedAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;
using VerifyMultiFactorAuthenticationUpdateResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.VerifyMultiFactorAuthenticationUpdateError>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class MultiFactorAuthenticationControllerTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string ControllerPath = "mfa";
    private const string AccessToken = "access-token";

    private readonly DateTime _currentDateTime = new(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly IIdentityAdministrationService _mockedAuthorisationService =
        Substitute.For<IIdentityAdministrationService>();
    private readonly ILoginService _mockedLoginService = Substitute.For<ILoginService>();
    private readonly HttpClient _client;

    private readonly AuthenticationCredentialsDto _validAuthenticationCredentials = new()
    {
        AccessToken = "access-token",
        RefreshToken = "refresh-token",
    };

    public MultiFactorAuthenticationControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var accessTokenProviderMock = Substitute.For<IAccessTokenProvider>();
        accessTokenProviderMock.GetAccessTokenAsync().Returns(AccessToken);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ILoginService>();
                    services.RemoveAll<IIdentityAdministrationService>();
                    services.AddSingleton(_mockedLoginService);
                    services.AddSingleton(_mockedAuthorisationService);
                    services.AddSingleton<IDateTimeProvider>(
                        new FakeDateTimeProvider(_currentDateTime)
                    );
                    services.AddSingleton(_ => accessTokenProviderMock);
                });
                builder.ConfigureNoDatabase();
                builder.UseSetting("AWS:LoadSecrets", $"{false}");
                builder.UseSetting(
                    $"{DevAuthenticationOptions.SectionName}:{nameof(DevAuthenticationOptions.IsEnabled)}",
                    $"{true}"
                );
            })
            .CreateClient();

        _mockedLoginService
            .RefreshAuthenticationToken(
                Arg.Any<RefreshAuthenticationTokenCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(InitiatedAuthenticationResult.Ok(_validAuthenticationCredentials));
        _mockedLoginService
            .SignOut(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _mockedLoginService
            .InitiateForgottenPasswordProcedure(
                Arg.Any<InitiateForgottenPasswordProcedureCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<InitiateForgottenPasswordProcedureError>.Ok());
        _mockedLoginService
            .ConfirmNewPassword(Arg.Any<ConfirmNewPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ConfirmNewPasswordError>.Ok());
    }

    [Fact]
    public async Task InitiateUpdateMultiFactorAuthentication_ShouldReturnValuesFromServiceMethod()
    {
        var expectedValues = new MultiFactorAuthenticationUpdateDto()
        {
            OtpAuthUri = new Uri(
                "otpauth://totp/MyApp:test@example.com?secret=JBSWY3DPEHPK3PXP&issuer=MyApp"
            ),
        };
        _mockedAuthorisationService
            .InitiateUpdateMultiFactorAuthentication(AccessToken, Arg.Any<CancellationToken>())
            .Returns(expectedValues);
        var response = await SendInitiateUpdateMultiFactorAuthenticationRequest();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<MultiFactorAuthenticationUpdateDto>(
            TestContext.Current.CancellationToken
        );
        content.ShouldBe(expectedValues);
    }

    [Fact]
    public async Task VerifyMultiFactorAuthenticationUpdate_ShouldPassParametersToServiceMethod()
    {
        MockVerifyMultiFactorAuthenticationUpdate(VerifyMultiFactorAuthenticationUpdateResult.Ok());
        var code = "123456";
        _ = await SendVerifyMultiFactorAuthenticationUpdateRequest(x => x with { Code = code });
        await _mockedAuthorisationService
            .Received(1)
            .VerifyMultiFactorAuthenticationUpdate(
                AccessToken,
                new VerifyMultiFactorAuthenticationUpdateCommand() { Code = code },
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task VerifyMultiFactorAuthenticationUpdate_WhenSuccess_ShouldReturn200()
    {
        MockVerifyMultiFactorAuthenticationUpdate(VerifyMultiFactorAuthenticationUpdateResult.Ok());
        var response = await SendVerifyMultiFactorAuthenticationUpdateRequest();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task VerifyMultiFactorAuthenticationUpdate_WhenInvalidCode_ShouldReturn400()
    {
        MockVerifyMultiFactorAuthenticationUpdate(
            VerifyMultiFactorAuthenticationUpdateResult.Err(
                new VerifyMultiFactorAuthenticationUpdateError.InvalidCode()
            )
        );
        var response = await SendVerifyMultiFactorAuthenticationUpdateRequest();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private void MockVerifyMultiFactorAuthenticationUpdate(
        VerifyMultiFactorAuthenticationUpdateResult expectedValues
    )
    {
        _mockedAuthorisationService
            .VerifyMultiFactorAuthenticationUpdate(
                AccessToken,
                Arg.Any<VerifyMultiFactorAuthenticationUpdateCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(expectedValues);
    }

    private async Task<HttpResponseMessage> SendInitiateUpdateMultiFactorAuthenticationRequest()
    {
        using var content = new StringContent(string.Empty);
        return await _client.PostAsync(
            new Uri($"/{ControllerPath}/update", UriKind.Relative),
            content,
            TestContext.Current.CancellationToken
        );
    }

    private Task<HttpResponseMessage> SendVerifyMultiFactorAuthenticationUpdateRequest(
        Func<
            VerifyMultiFactorAuthenticationUpdateCommand,
            VerifyMultiFactorAuthenticationUpdateCommand
        >? overrides = null
    )
    {
        var defaultCommand = new VerifyMultiFactorAuthenticationUpdateCommand() { Code = "123456" };
        var command = overrides is null ? defaultCommand : overrides(defaultCommand);
        return _client.PostAsJsonAsync(
            new Uri($"/{ControllerPath}/verify", UriKind.Relative),
            command,
            TestContext.Current.CancellationToken
        );
    }
}
