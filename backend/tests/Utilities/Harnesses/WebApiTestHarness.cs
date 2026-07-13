using System.Security.Claims;
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

    public WebApiTestHarness(
        AppDbContext context,
        ApiFactory apiFactory,
        Func<HttpClient, TService> initialiser
    )
    {
        Context = context;
        _apiFactory = apiFactory;
        Service = initialiser(apiFactory.CreateClient());
    }

    public IServiceTestHarness<TService> UpdateCurrentUser(Func<CurrentUser, CurrentUser> update)
    {
        _apiFactory.AuthOptions.Claims.Add(new Claim("Example Claim", "Example Claim"));
        return this;
    }
}
