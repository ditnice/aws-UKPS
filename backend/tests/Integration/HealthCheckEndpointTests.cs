using Microsoft.AspNetCore.Mvc.Testing;

namespace UKPS.Api.Tests.Integration;

public class HealthCheckEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient;

    public HealthCheckEndpointTests(WebApplicationFactory<Program> apiFactory)
    {
        ArgumentNullException.ThrowIfNull(apiFactory);

        _httpClient = apiFactory.CreateClient();
    }

    [Fact]
    public async Task HealthCheckEndpoint_ShouldReturnSuccessfullyStatus()
    {
        var healthCheckUrl = new Uri("/health");
        HttpResponseMessage response = await _httpClient.GetAsync(healthCheckUrl);

        Assert.True(response.IsSuccessStatusCode);
    }
}
