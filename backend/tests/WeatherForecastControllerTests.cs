namespace UKPS.Api.Tests;

using UKPS.Api;
using UKPS.Api.Controllers;

public class WeatherForecastControllerTests
{
    private static readonly string[] ExpectedSummaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [Fact]
    public void Get_ReturnsFiveForecasts()
    {
        WeatherForecastController controller = new();

        WeatherForecast[] forecasts = controller.Get().ToArray();

        Assert.Equal(5, forecasts.Length);
    }

    [Fact]
    public void Get_ReturnsForecastsForNextFiveDays()
    {
        WeatherForecastController controller = new();
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        WeatherForecast[] forecasts = controller.Get().ToArray();

        Assert.Collection(
            forecasts,
            forecast => Assert.Equal(today.AddDays(1), forecast.Date),
            forecast => Assert.Equal(today.AddDays(2), forecast.Date),
            forecast => Assert.Equal(today.AddDays(3), forecast.Date),
            forecast => Assert.Equal(today.AddDays(4), forecast.Date),
            forecast => Assert.Equal(today.AddDays(5), forecast.Date));
    }

    [Fact]
    public void Get_ReturnsForecastsWithExpectedTemperatureAndSummaryRanges()
    {
        WeatherForecastController controller = new();

        WeatherForecast[] forecasts = controller.Get().ToArray();

        Assert.All(forecasts, forecast =>
        {
            Assert.InRange(forecast.TemperatureC, -20, 54);
            Assert.Contains(forecast.Summary, ExpectedSummaries);
            Assert.Equal(32 + (int)(forecast.TemperatureC / 0.5556), forecast.TemperatureF);
        });
    }
}
