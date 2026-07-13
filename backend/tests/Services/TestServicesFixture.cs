using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using UKPS.Api.Data;
using UKPS.Api.Enums;
using UKPS.Api.Services;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Tests.Services;

internal sealed class TestServicesFixture
{
    public IOrganisationMembershipService OrganisationMembershipService =>
        _serviceProvider.GetRequiredService<IOrganisationMembershipService>();

    public IOrganisationService OrganisationService =>
        _serviceProvider.GetRequiredService<IOrganisationService>();

    public IUserService UserService => _serviceProvider.GetRequiredService<IUserService>();

    private readonly IServiceProvider _serviceProvider;

    public TestServicesFixture(AppDbContext context, CurrentUser currentUserInfo)
    {
        var mockCurrentUserInfoService = Substitute.For<ICurrentUserInfoService>();
        mockCurrentUserInfoService.GetCurrentUserInfo().Returns(currentUserInfo);
        _serviceProvider = new ServiceCollection()
            .AddScoped(_ => context)
            .AddUkpsServices()
            .AddTransient(_ => mockCurrentUserInfoService)
            .BuildServiceProvider();
    }

    public static TestServicesFixture Create(
        AppDbContext context,
        Func<CurrentUser, CurrentUser>? updateCurrentUserInfo = null
    )
    {
        var defaultUserInfo = new CurrentUser { OrganisationId = 1, UserRole = UserRole.Super };
        var updatedCurrentUserInfo = updateCurrentUserInfo is not null
            ? updateCurrentUserInfo(defaultUserInfo)
            : defaultUserInfo;
        return new TestServicesFixture(context, updatedCurrentUserInfo);
    }
}
