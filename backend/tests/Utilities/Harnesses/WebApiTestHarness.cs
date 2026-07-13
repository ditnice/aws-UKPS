using System.Globalization;
using System.Security.Claims;
using UKPS.Api.Controllers.Utilities;
using UKPS.Api.Data;
using UKPS.Api.Services.Interfaces;
using UKPS.Api.Tests.Fixtures;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal sealed class WebApiTestHarness<TService> : IServiceTestHarness<TService>
    where TService : notnull
{
    private readonly ApiFactory _apiFactory;

    public TService Service { get; }
    public AppDbContext Context { get; }

    private CurrentUser _currentUser = AuthorisationTestConstants.DefaultCurrentUser;

    public WebApiTestHarness(
        AppDbContext context,
        ApiFactory apiFactory,
        Func<HttpClient, TService> initialiser
    )
    {
        Context = context;
        _apiFactory = apiFactory;
        Service = initialiser(apiFactory.CreateClient());
        InitialiseClaimsForUser(_currentUser);
    }

    public IServiceTestHarness<TService> UpdateCurrentUser(Func<CurrentUser, CurrentUser> update)
    {
        _currentUser = update(_currentUser);
        InitialiseClaimsForUser(_currentUser);
        return this;
    }

    private void InitialiseClaimsForUser(CurrentUser currentUser)
    {
        _apiFactory.AuthOptions.Claims.Clear();
        _apiFactory.AuthOptions.Claims.Add(
            new Claim(
                UkpsClaimTypes.OrganisationId,
                currentUser.OrganisationId.ToString(CultureInfo.InvariantCulture)
            )
        );
        _apiFactory.AuthOptions.Claims.Add(
            new Claim(UkpsClaimTypes.UserRole, currentUser.UserRole.ToString())
        );
    }
}
