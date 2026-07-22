using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using UKPS.Api.Application;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Persistence;
using UKPS.Api.Tests.Utilities.MockInternalServices;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal sealed class ServiceTestHarness<TService> : IServiceTestHarness<TService>
    where TService : notnull
{
    public TService Service =>
        _serviceCollection.BuildServiceProvider().GetRequiredService<TService>();
    public AppDbContext Context { get; }
    public MockEmailService Emails { get; } = new MockEmailService();

    private readonly ICurrentUserInfoService _mockCurrentUserInfoService;
    private CurrentUser _currentUser = AuthorisationTestConstants.DefaultCurrentUser;
    private IDateTimeProvider _timeProvider = new SystemDateTimeProvider();
    private IServiceCollection _serviceCollection;

    public ServiceTestHarness(AppDbContext context)
    {
        var webIdentityAdministrationService = Substitute.For<IWebIdentityAdministrationService>();

        Context = context;
        _mockCurrentUserInfoService = Substitute.For<ICurrentUserInfoService>();
        _mockCurrentUserInfoService.GetCurrentUserInfo().Returns(_currentUser);
        _serviceCollection = new ServiceCollection()
            .AddScoped(_ => context)
            .AddUkpsServices()
            .AddTransient(_ => _mockCurrentUserInfoService)
            .AddTransient(_ => webIdentityAdministrationService)
            .AddTransient(_ => Emails.Mock)
            .AddSingleton(_ => _timeProvider)
            .AddLogging();
    }

    public IServiceTestHarness<TService> UpdateCurrentUser(Func<CurrentUser, CurrentUser> update)
    {
        _currentUser = update(_currentUser);
        _mockCurrentUserInfoService.GetCurrentUserInfo().Returns(_currentUser);
        return this;
    }

    public IServiceTestHarness<TService> UpdateCurrentTime(DateTime dateTime)
    {
        _timeProvider = new FakeDateTimeProvider(dateTime);
        return this;
    }

    public IServiceTestHarness<TService> ConfigureServices(
        Func<IServiceCollection, IServiceCollection> func
    )
    {
        _serviceCollection = func(_serviceCollection);
        return this;
    }
}
