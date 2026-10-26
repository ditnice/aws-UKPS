using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi;
using UKPS.Api.WebApi.Controllers;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.Tests.WebApi.Authorisation;

public class DeactivatedMembershipEndpointAccessTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DeactivatedMembershipEndpointAccessTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureNoDatabase();
            builder.UseSetting("AWS:LoadSecrets", $"{false}");
        });
    }

    [Fact]
    public async Task AuthorisedEndpoints_WhenMembershipDeactivated_ShouldOnlyAllowChampionContacts()
    {
        IServiceProvider services = _factory.Services;
        EndpointDataSource endpointDataSource = services.GetRequiredService<EndpointDataSource>();
        IAuthorizationPolicyProvider policyProvider =
            services.GetRequiredService<IAuthorizationPolicyProvider>();
        IAuthorizationService authorizationService =
            services.GetRequiredService<IAuthorizationService>();

        ClaimsPrincipal deactivatedUser = new(
            new ClaimsIdentity(
                [
                    new Claim(UkpsClaimTypes.Username, "deactivated-user"),
                    new Claim(
                        UkpsClaimTypes.OrganisationSelectionFailure,
                        $"{AuthenticationFailCode.MembershipDeactivated}"
                    ),
                ],
                authenticationType: "Test"
            )
        );

        List<string> accessibleActions = [];

        foreach (Endpoint endpoint in endpointDataSource.Endpoints)
        {
            if (
                endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not { } action
                || endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null
            )
            {
                continue;
            }

            AuthorizationPolicy? policy = await AuthorizationPolicy.CombineAsync(
                policyProvider,
                endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
                endpoint.Metadata.GetOrderedMetadata<AuthorizationPolicy>()
            );

            // Endpoints without authorisation metadata are public by design.
            if (policy is null)
            {
                continue;
            }

            AuthorizationResult result = await authorizationService.AuthorizeAsync(
                deactivatedUser,
                policy
            );

            if (result.Succeeded)
            {
                accessibleActions.Add($"{action.ControllerName}.{action.ActionName}");
            }
        }

        accessibleActions.ShouldBe([
            $"CurrentUserOrganisationChampion.{nameof(CurrentUserOrganisationChampionController.GetCurrentOrganisationChampions)}",
        ]);
    }
}
