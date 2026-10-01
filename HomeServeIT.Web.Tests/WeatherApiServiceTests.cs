using System.Net;
using System.Text;
using HomeServeIT.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace HomeServeIT.Web.Tests;

public sealed class WeatherApiServiceTests
{
    [Fact]
    public async Task GetsLiveWeatherAndScheduledForecastFromWeatherApi()
    {
        var targetDate = DateTime.Today.AddDays(1);
        var handler = new WeatherApiHandler(targetDate);
        using var client = new HttpClient(handler);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new WeatherService(
            client,
            cache,
            NullLogger<WeatherService>.Instance,
            new WeatherApiOptions { ApiKey = "test-api-key" });

        var result = await service.GetWeatherAsync("Davao City", targetDate);

        Assert.NotNull(result);
        Assert.Equal("Davao City", result.LocationName);
        Assert.Equal(31.2, result.CurrentTemperatureCelsius);
        Assert.Equal("Partly cloudy", result.CurrentSummary);
        Assert.Equal(24, result.VisitMinimumTemperatureCelsius);
        Assert.Equal(29, result.VisitMaximumTemperatureCelsius);
        Assert.Equal(80, result.VisitPrecipitationProbabilityPercent);
        Assert.Equal("Light rain", result.VisitSummary);
        Assert.Single(handler.Requests);
        Assert.Contains("key=test-api-key", handler.Requests[0].Query);
        Assert.Contains("q=Davao%20City", handler.Requests[0].Query);
        Assert.DoesNotContain("street", handler.Requests[0].Query, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DoesNotCallProviderWhenApiKeyIsMissing()
    {
        var handler = new WeatherApiHandler(DateTime.Today.AddDays(1));
        using var client = new HttpClient(handler);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new WeatherService(
            client,
            cache,
            NullLogger<WeatherService>.Instance,
            new WeatherApiOptions());

        var result = await service.GetWeatherAsync("Davao City", DateTime.Today.AddDays(1));

        Assert.Null(result);
        Assert.Empty(handler.Requests);
    }

    private sealed class WeatherApiHandler(DateTime targetDate) : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            var body = $$"""
            {
              "location": { "name": "Davao City" },
              "current": {
                "last_updated": "2026-10-02 12:00",
                "temp_c": 31.2,
                "feelslike_c": 34.1,
                "condition": { "text": "Partly cloudy" }
              },
              "forecast": {
                "forecastday": [
                  {
                    "date": "{{targetDate:yyyy-MM-dd}}",
                    "day": {
                      "mintemp_c": 24,
                      "maxtemp_c": 29,
                      "daily_chance_of_rain": 80,
                      "condition": { "text": "Light rain" }
                    }
                  }
                ]
              }
            }
            """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
