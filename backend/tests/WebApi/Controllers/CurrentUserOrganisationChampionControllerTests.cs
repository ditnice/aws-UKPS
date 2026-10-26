using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.CustomResponses;
using UKPS.Api.WebApi.InternalServices.Authentication;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class CurrentUserOrganisationChampionControllerTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string ChampionContactsUrl = "/users/me/organisations/current/champions";
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CurrentUserOrganisationChampionControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        var claims = new DevAuthenticationClaims().UpdateClaim(
            new Claim(UkpsClaimTypes.UserRole, UserRole.Standard.ToString())
        );
        _factory = factory.WithWebHostBuilder(builder =>
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
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DevAuthenticationClaims>();
                services.AddSingleton(claims);
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetCurrentOrganisationChampions_ShouldReturnEmailsForStandardUser()
    {
        IReadOnlyCollection<string> expected = ["a.champion@example.com", "z.champion@example.com"];
        _mockUserService
            .GetCurrentOrganisationChampionEmails(Arg.Any<CancellationToken>())
            .Returns(
                Result<IReadOnlyCollection<string>, GetCurrentOrganisationChampionsError>.Ok(
                    expected
                )
            );

        var response = await _client.GetAsync(
            new Uri(ChampionContactsUrl, UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<string[]>(
            TestContext.Current.CancellationToken
        );
        content.ShouldBe(expected);
        await _mockUserService
            .Received(1)
            .GetCurrentOrganisationChampionEmails(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCurrentOrganisationChampions_ShouldReturnEmptyCollection_WhenNoChampionsExist()
    {
        _mockUserService
            .GetCurrentOrganisationChampionEmails(Arg.Any<CancellationToken>())
            .Returns(
                Result<IReadOnlyCollection<string>, GetCurrentOrganisationChampionsError>.Ok([])
            );

        var response = await _client.GetAsync(
            new Uri(ChampionContactsUrl, UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<string[]>(
            TestContext.Current.CancellationToken
        );
        content.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCurrentOrganisationChampions_ShouldReturnNeutralForbiddenProblem_WhenNotAllowed()
    {
        _mockUserService
            .GetCurrentOrganisationChampionEmails(Arg.Any<CancellationToken>())
            .Returns(
                Result<IReadOnlyCollection<string>, GetCurrentOrganisationChampionsError>.Err(
                    new GetCurrentOrganisationChampionsError.NotAllowed()
                )
            );

        var response = await _client.GetAsync(
            new Uri(ChampionContactsUrl, UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var content = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            TestContext.Current.CancellationToken
        );
        content.ShouldNotBeNull();
        content.Status.ShouldBe(403);
        content.Title.ShouldBe("Forbidden");
        content.Detail.ShouldBe("You are not authorised to view champion contacts.");
    }

    [Fact]
    public void GetCurrentOrganisationChampions_ShouldDeclareSuccessForbiddenAndAuthenticationResponses()
    {
        var descriptions =
            _factory.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
        var action = descriptions
            .ApiDescriptionGroups.Items.SelectMany(group => group.Items)
            .Single(x =>
                string.Equals(x.HttpMethod, "GET", StringComparison.Ordinal)
                && string.Equals(
                    x.RelativePath,
                    ChampionContactsUrl.TrimStart('/'),
                    StringComparison.Ordinal
                )
            );

        action.SupportedResponseTypes.ShouldContain(x =>
            x.StatusCode == 200 && x.Type == typeof(IReadOnlyCollection<string>)
        );
        action.SupportedResponseTypes.ShouldContain(x => x.StatusCode == 403);
        action.SupportedResponseTypes.ShouldContain(x =>
            x.StatusCode == 401 && x.Type == typeof(AuthenticationProblemDetails)
        );
    }
}
