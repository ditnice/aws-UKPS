namespace UKPS.Api.Tests;

using UKPS.Api;

public class WeatherForecastTests
{
    [Theory]
    [InlineData(0, 32)]
    [InlineData(20, 67)]
    [InlineData(-10, 15)]
    public void TemperatureF_ReturnsExpectedConvertedValue(int temperatureC, int expectedTemperatureF)
    {
        WeatherForecast forecast = new()
        {
            TemperatureC = temperatureC
        };

        Assert.Equal(expectedTemperatureF, forecast.TemperatureF);
    }
}
