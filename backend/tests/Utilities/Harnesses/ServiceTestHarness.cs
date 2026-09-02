using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    public MockEmailService Emails { get; } = new();
    public MockAmazonCognitoIdentityProvider Cognito { get; } = new();
    public MockLoggerProvider Logs { get; } = new();

    private readonly ICurrentUserInfoService _mockCurrentUserInfoService;
    private CurrentUser _currentUser = AuthorisationTestConstants.DefaultCurrentUser;
    private IDateTimeProvider _timeProvider = new SystemDateTimeProvider();
    private IServiceCollection _serviceCollection;
    private readonly AppDbContext _appContext;

    public ServiceTestHarness(AppDbContext context)
    {
        _appContext = context;
        _mockCurrentUserInfoService = Substitute.For<ICurrentUserInfoService>();
        _mockCurrentUserInfoService.GetCurrentUserInfo().Returns(_currentUser);
        _serviceCollection = new ServiceCollection()
            .AddScoped(_ => GetClearedContext())
            .AddUkpsServices()
            .AddTransient(_ => _mockCurrentUserInfoService)
            .AddTransient(_ => Cognito.Mock)
            .AddTransient(_ => Emails.Mock)
            .AddSingleton(_ => _timeProvider)
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddProvider(Logs);
            });
    }

    public ServiceTestHarness(IServiceTestHarness harness)
        : this(harness.GetClearedContext())
    {
        Emails = harness.Emails;
        Cognito = harness.Cognito;
    }

    public AppDbContext GetClearedContext()
    {
        _appContext.ChangeTracker.Clear();
        return _appContext;
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
