using UKPS.Api.Services.Interfaces;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Services.Organisations;

[Collection(DatabaseCollection.Name)]
public class OrganisationServiceApplicationTests(PostgresFixture fixture)
    : OrganisationServiceTests(fixture)
{
    internal override IServiceTestHarness<IOrganisationService> ServiceTestHarness =>
        new ServiceTestHarness<IOrganisationService>(Context);
}
