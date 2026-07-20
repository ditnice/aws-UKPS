using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using UKPS.Api.Application;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Data;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal sealed class ServiceTestHarness<TService> : IServiceTestHarness<TService>
    where TService : notnull
{
    public TService Service => _serviceProvider.GetRequiredService<TService>();
    public AppDbContext Context { get; }

    private readonly ICurrentUserInfoService _mockCurrentUserInfoService;
    private readonly ServiceProvider _serviceProvider;
    private CurrentUser _currentUser = AuthorisationTestConstants.DefaultCurrentUser;

    public ServiceTestHarness(AppDbContext context)
    {
        Context = context;
        _mockCurrentUserInfoService = Substitute.For<ICurrentUserInfoService>();
        _mockCurrentUserInfoService.GetCurrentUserInfo().Returns(_currentUser);
        _serviceProvider = new ServiceCollection()
            .AddScoped(_ => context)
            .AddUkpsServices()
            .AddTransient(_ => _mockCurrentUserInfoService)
            .BuildServiceProvider();
    }

    public IServiceTestHarness<TService> UpdateCurrentUser(Func<CurrentUser, CurrentUser> update)
    {
        _currentUser = update(_currentUser);
        _mockCurrentUserInfoService.GetCurrentUserInfo().Returns(_currentUser);
        return this;
    }
}
