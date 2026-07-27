using Microsoft.Extensions.DependencyInjection;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence;
using UKPS.Api.Tests.Utilities.MockInternalServices;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal interface IServiceTestHarness<TService>
    where TService : notnull
{
    TService Service { get; }
    AppDbContext Context { get; }
    MockEmailService Emails { get; }

    IServiceTestHarness<TService> UpdateCurrentUser(Func<CurrentUser, CurrentUser> update);
    IServiceTestHarness<TService> UpdateCurrentTime(DateTime dateTime);
    IServiceTestHarness<TService> ConfigureServices(
        Func<IServiceCollection, IServiceCollection> func
    );
}
