using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi;
using UKPS.Api.WebApi.InternalServices.Authentication;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.Tests.WebApi.Authorisation;

// The organisation is authorised before it is looked up, so these requests need no database.
public class SelectedOrganisationMismatchTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int SelectedOrganisationId = 1;
    private const int OtherMemberOrganisationId = 2;
    private const int NonMemberOrganisationId = 3;

    private readonly WebApplicationFactory<Program> _factory;

    public SelectedOrganisationMismatchTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetOrganisationById_WhenMemberButNotSelected_ShouldReturnConflictWithFailCode()
    {
        HttpClient client = CreateClient();

        var response = await client.GetAsync(
            new Uri($"/organisations/{OtherMemberOrganisationId}", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        using JsonDocument content = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)
        );
        content
            .RootElement.GetProperty("code")
            .GetString()
            .ShouldBe(nameof(AuthenticationFailCode.SelectedOrganisationMismatch));
        content
            .RootElement.GetProperty("organisationId")
            .GetInt32()
            .ShouldBe(OtherMemberOrganisationId);
    }

    [Fact]
    public async Task GetOrganisationById_WhenNotAMember_ShouldReturnForbidden()
    {
        HttpClient client = CreateClient();

        var response = await client.GetAsync(
            new Uri($"/organisations/{NonMemberOrganisationId}", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private HttpClient CreateClient()
    {
        var claims = new DevAuthenticationClaims()
            .UpdateClaim(new Claim(UkpsClaimTypes.UserRole, $"{UserRole.Standard}"))
            .UpdateClaim(new Claim(UkpsClaimTypes.OrganisationId, $"{SelectedOrganisationId}"));
        claims.Claims.Add(
            new Claim(UkpsClaimTypes.SelectableOrganisationId, $"{SelectedOrganisationId}")
        );
        claims.Claims.Add(
            new Claim(UkpsClaimTypes.SelectableOrganisationId, $"{OtherMemberOrganisationId}")
        );

        return _factory
            .WithWebHostBuilder(builder =>
            {
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
                    // Resolving the real email service creates an SQS client, which needs AWS
                    // credentials.
                    services.RemoveAll<IEmailService>();
                    services.AddSingleton(Substitute.For<IEmailService>());
                });
            })
            .CreateClient();
    }
}
