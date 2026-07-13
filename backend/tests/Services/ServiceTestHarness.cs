using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using UKPS.Api.Data;
using UKPS.Api.Enums;
using UKPS.Api.Services;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Tests.Services;

internal sealed class ServiceTestHarness<TService>
    where TService : notnull
{
    public TService Service => _serviceProvider.GetRequiredService<TService>();
    public AppDbContext Context { get; }

    private readonly ICurrentUserInfoService _mockCurrentUserInfoService;
    private readonly ServiceProvider _serviceProvider;
    private CurrentUser _currentUser = new CurrentUser
    {
        OrganisationId = 1,
        UserRole = UserRole.Super,
    };

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

    public ServiceTestHarness<TService> UpdateCurrentUser(Func<CurrentUser, CurrentUser> update)
    {
        _currentUser = update(_currentUser);
        return this;
    }
}
