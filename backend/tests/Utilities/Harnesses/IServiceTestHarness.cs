using UKPS.Api.Data;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal interface IServiceTestHarness<TService>
    where TService : notnull
{
    TService Service { get; }
    AppDbContext Context { get; }
    IServiceTestHarness<TService> UpdateCurrentUser(Func<CurrentUser, CurrentUser> update);
}
