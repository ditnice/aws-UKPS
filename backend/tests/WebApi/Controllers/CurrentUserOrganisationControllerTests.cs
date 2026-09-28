using System.Net;
using System.Net.Http.Json;
using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Organisations.Dtos;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class CurrentUserOrganisationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string CurrentUserUrl = "/users/me";
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();
    private readonly HttpClient _client;

    public CurrentUserOrganisationControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IUserService>();
                    services.AddSingleton(_mockUserService);
                });
                builder.ConfigureNoDatabase();
                builder.UseSetting("AWS:LoadSecrets", $"{false}");
                builder.UseSetting(
                    $"{DevAuthenticationOptions.SectionName}:{nameof(DevAuthenticationOptions.IsEnabled)}",
                    $"{true}"
                );
            })
            .CreateClient();

        _mockUserService
            .UpdateCurrentOrganisation(
                Arg.Any<UpdateCurrentOrganisationCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<UpdateCurrentOrganisationError>.Ok());
    }

    [Fact]
    public async Task GetCurrentUserOrganisations_ShouldReturnOrganisations()
    {
        IReadOnlyCollection<OrganisationListDto> expected =
        [
            new OrganisationListDto { Id = 1, OrganisationName = "Organisation1" },
            new OrganisationListDto { Id = 2, OrganisationName = "Organisation2" },
        ];
        _mockUserService
            .GetCurrentUserOrganisations(Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await _client.GetAsync(
            new Uri($"{CurrentUserUrl}/organisations", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<
            IReadOnlyCollection<OrganisationListDto>
        >(TestContext.Current.CancellationToken);
        content.ShouldBe(expected);
    }

    [Fact]
    public async Task UpdateCurrentOrganisation_ShouldCallTheServiceWithProvidedArguments()
    {
        var testOrgId = 40;
        var response = await MakeUpdateCurrentOrganisation(x =>
            x with
            {
                OrganisationId = testOrgId,
            }
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await _mockUserService
            .Received(1)
            .UpdateCurrentOrganisation(
                new UpdateCurrentOrganisationCommand() { OrganisationId = testOrgId },
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task UpdateCurrentOrganisation_WhenBadRequestError_ShouldReturnBadRequestResponse()
    {
        _mockUserService
            .UpdateCurrentOrganisation(
                Arg.Any<UpdateCurrentOrganisationCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Result<UpdateCurrentOrganisationError>.Err(
                    new UpdateCurrentOrganisationError.ProvidedOrganisationWasNotValid()
                )
            );
        var response = await MakeUpdateCurrentOrganisation();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<HttpResponseMessage> MakeUpdateCurrentOrganisation(
        Func<UpdateCurrentOrganisationCommand, UpdateCurrentOrganisationCommand>? modifier = null
    )
    {
        var defaultCommand = new UpdateCurrentOrganisationCommandFaker().Generate();
        var command = modifier is not null ? modifier(defaultCommand) : defaultCommand;
        return await _client.PatchAsJsonAsync(
            new Uri($"{CurrentUserUrl}/current-organisation", UriKind.Relative),
            command,
            TestContext.Current.CancellationToken
        );
    }

    private sealed class UpdateCurrentOrganisationCommandFaker
        : Faker<UpdateCurrentOrganisationCommand>
    {
        public UpdateCurrentOrganisationCommandFaker()
        {
            RuleFor(x => x.OrganisationId, f => f.Random.Int(1));
        }
    }
}
