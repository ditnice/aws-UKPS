using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi;
using UKPS.Api.WebApi.InternalServices.Authentication;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.Tests.WebApi.Authorisation;

public class OrganisationSelectionAuthorisationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();
    private readonly WebApplicationFactory<Program> _factory;

    public OrganisationSelectionAuthorisationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;

        _mockUserService.GetCurrentUserOrganisations(Arg.Any<CancellationToken>()).Returns([]);
        _mockUserService
            .UpdateCurrentOrganisation(
                Arg.Any<UpdateCurrentOrganisationCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<UpdateCurrentOrganisationError>.Ok());
        _mockUserService
            .GetCurrentOrganisationChampionEmails(Arg.Any<CancellationToken>())
            .Returns(
                Result<IReadOnlyCollection<string>, GetCurrentOrganisationChampionsError>.Ok([
                    "champion@example.com",
                ])
            );
    }

    [Theory]
    [InlineData(AuthenticationFailCode.SelectedOrganisationRequired)]
    [InlineData(AuthenticationFailCode.SelectedOrganisationIsNotValid)]
    [InlineData(AuthenticationFailCode.MembershipDeactivated)]
    public async Task DefaultPolicyEndpoint_WhenOrganisationNotSelected_ShouldReturnUnauthorisedWithFailCode(
        AuthenticationFailCode failCode
    )
    {
        HttpClient client = CreateClientRequiringOrganisationSelection(failCode);

        var response = await client.GetAsync(
            new Uri("/users/me", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using JsonDocument content = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)
        );
        content.RootElement.GetProperty("code").GetString().ShouldBe(failCode.ToString());
        await _mockUserService.DidNotReceive().GetCurrentUser(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCurrentUserOrganisations_WhenOrganisationNotSelected_ShouldBeAccessible()
    {
        HttpClient client = CreateClientRequiringOrganisationSelection(
            AuthenticationFailCode.SelectedOrganisationRequired
        );

        var response = await client.GetAsync(
            new Uri("/users/me/organisations", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateCurrentOrganisation_WhenOrganisationNotSelected_ShouldBeAccessible()
    {
        HttpClient client = CreateClientRequiringOrganisationSelection(
            AuthenticationFailCode.SelectedOrganisationRequired
        );

        var response = await client.PatchAsJsonAsync(
            new Uri("/users/me/current-organisation", UriKind.Relative),
            new UpdateCurrentOrganisationCommand { OrganisationId = 1 },
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(AuthenticationFailCode.SelectedOrganisationRequired)]
    [InlineData(AuthenticationFailCode.SelectedOrganisationIsNotValid)]
    public async Task GetCurrentOrganisationChampions_WhenOrganisationNotSelected_ShouldReturnUnauthorisedWithFailCode(
        AuthenticationFailCode failCode
    )
    {
        HttpClient client = CreateClientRequiringOrganisationSelection(failCode);

        var response = await client.GetAsync(
            new Uri("/users/me/current-organisation/champions", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using JsonDocument content = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)
        );
        content.RootElement.GetProperty("code").GetString().ShouldBe(failCode.ToString());
        await _mockUserService
            .DidNotReceive()
            .GetCurrentOrganisationChampionEmails(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCurrentOrganisationChampions_WhenMembershipDeactivated_ShouldBeAccessible()
    {
        HttpClient client = CreateClientRequiringOrganisationSelection(
            AuthenticationFailCode.MembershipDeactivated
        );

        var response = await client.GetAsync(
            new Uri("/users/me/current-organisation/champions", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (
            await response.Content.ReadFromJsonAsync<string[]>(
                TestContext.Current.CancellationToken
            )
        ).ShouldBe(["champion@example.com"]);
        await _mockUserService
            .Received(1)
            .GetCurrentOrganisationChampionEmails(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCurrentUserOrganisations_WhenMembershipDeactivated_ShouldReturnUnauthorisedWithFailCode()
    {
        HttpClient client = CreateClientRequiringOrganisationSelection(
            AuthenticationFailCode.MembershipDeactivated
        );

        var response = await client.GetAsync(
            new Uri("/users/me/organisations", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using JsonDocument content = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)
        );
        content
            .RootElement.GetProperty("code")
            .GetString()
            .ShouldBe(AuthenticationFailCode.MembershipDeactivated.ToString());
        await _mockUserService
            .DidNotReceive()
            .GetCurrentUserOrganisations(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCurrentOrganisation_WhenMembershipDeactivated_ShouldReturnUnauthorisedWithFailCode()
    {
        HttpClient client = CreateClientRequiringOrganisationSelection(
            AuthenticationFailCode.MembershipDeactivated
        );

        var response = await client.PatchAsJsonAsync(
            new Uri("/users/me/current-organisation", UriKind.Relative),
            new UpdateCurrentOrganisationCommand { OrganisationId = 1 },
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using JsonDocument content = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)
        );
        content
            .RootElement.GetProperty("code")
            .GetString()
            .ShouldBe(AuthenticationFailCode.MembershipDeactivated.ToString());
        await _mockUserService
            .DidNotReceive()
            .UpdateCurrentOrganisation(
                Arg.Any<UpdateCurrentOrganisationCommand>(),
                Arg.Any<CancellationToken>()
            );
    }

    private HttpClient CreateClientRequiringOrganisationSelection(AuthenticationFailCode failCode)
    {
        var claims = new DevAuthenticationClaims();
        foreach (
            Claim organisationClaim in claims
                .Claims.Where(x =>
                    x.Type is UkpsClaimTypes.OrganisationId or UkpsClaimTypes.UserRole
                )
                .ToArray()
        )
        {
            claims.Claims.Remove(organisationClaim);
        }
        claims.UpdateClaim(new Claim(UkpsClaimTypes.OrganisationSelectionFailure, $"{failCode}"));

        return _factory
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
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<DevAuthenticationClaims>();
                    services.AddSingleton(claims);
                });
            })
            .CreateClient();
    }
}
