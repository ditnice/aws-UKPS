using UKPS.Api.Tests.Fixtures;

namespace UKPS.Api.Tests.Integration;

[Collection(DatabaseCollection.Name)]
public class HealthCheckEndpointTests
{
    private readonly HttpClient _httpClient;

    public HealthCheckEndpointTests(PostgresFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        _httpClient = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheckEndpoint_ShouldReturnSuccessfullyStatus()
    {
        var healthCheckUrl = new Uri("/health", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(healthCheckUrl);

        Assert.True(response.IsSuccessStatusCode);
    }
}
