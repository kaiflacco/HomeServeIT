using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace HomeServeIT.Web.Services;

public sealed class WeatherService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WeatherService> _logger;
    private readonly WeatherApiOptions _options;

    public WeatherService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<WeatherService> logger,
        WeatherApiOptions options)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _options = options;
    }

    public async Task<WeatherSnapshot?> GetWeatherAsync(
        string? city,
        DateTime scheduledDate,
        CancellationToken cancellationToken = default)
    {
        var normalizedCity = NormalizeCity(city);
        if (normalizedCity == null || string.IsNullOrWhiteSpace(_options.ApiKey))
            return null;

        try
        {
            var cacheKey = $"weatherapi:forecast:{normalizedCity.ToUpperInvariant()}";
            var response = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                // WeatherAPI permits caching current conditions for up to 60 minutes.
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                var url = $"https://api.weatherapi.com/v1/forecast.json?key={Uri.EscapeDataString(_options.ApiKey)}&q={Uri.EscapeDataString(normalizedCity)}&days=3&aqi=no&alerts=no";
                return await _httpClient.GetFromJsonAsync<WeatherApiResponse>(url, cancellationToken);
            });

            if (response?.Current == null)
                return null;

            var visitDay = response.Forecast?.ForecastDays?
                .FirstOrDefault(day => day.Date == scheduledDate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            return new WeatherSnapshot(
                LocationName: response.Location?.Name ?? normalizedCity,
                LastUpdatedLocal: ParseLocalDateTime(response.Current.LastUpdated),
                CurrentTemperatureCelsius: response.Current.TemperatureCelsius,
                CurrentFeelsLikeCelsius: response.Current.FeelsLikeCelsius,
                CurrentSummary: response.Current.Condition?.Text ?? "Current conditions unavailable",
                VisitMinimumTemperatureCelsius: visitDay?.Day?.MinimumTemperatureCelsius,
                VisitMaximumTemperatureCelsius: visitDay?.Day?.MaximumTemperatureCelsius,
                VisitPrecipitationProbabilityPercent: visitDay?.Day?.RainProbabilityPercent,
                VisitSummary: visitDay?.Day?.Condition?.Text);
        }
        catch (HttpRequestException error)
        {
            _logger.LogWarning("WeatherAPI request failed ({ErrorType}); weather will be omitted.", error.GetType().Name);
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("WeatherAPI request timed out; weather will be omitted.");
            return null;
        }
    }

    private static string? NormalizeCity(string? city)
    {
        if (string.IsNullOrWhiteSpace(city))
            return null;

        var normalized = city.Trim();
        return normalized.Length > 100 ? normalized[..100] : normalized;
    }

    private static DateTime? ParseLocalDateTime(string? value)
    {
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? result
            : null;
    }

    private sealed class WeatherApiResponse
    {
        [JsonPropertyName("location")]
        public WeatherLocation? Location { get; set; }

        [JsonPropertyName("current")]
        public CurrentWeather? Current { get; set; }

        [JsonPropertyName("forecast")]
        public Forecast? Forecast { get; set; }
    }

    private sealed class WeatherLocation
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class CurrentWeather
    {
        [JsonPropertyName("last_updated")]
        public string? LastUpdated { get; set; }

        [JsonPropertyName("temp_c")]
        public double TemperatureCelsius { get; set; }

        [JsonPropertyName("feelslike_c")]
        public double FeelsLikeCelsius { get; set; }

        [JsonPropertyName("condition")]
        public WeatherCondition? Condition { get; set; }
    }

    private sealed class Forecast
    {
        [JsonPropertyName("forecastday")]
        public List<ForecastDay>? ForecastDays { get; set; }
    }

    private sealed class ForecastDay
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("day")]
        public ForecastDaySummary? Day { get; set; }
    }

    private sealed class ForecastDaySummary
    {
        [JsonPropertyName("mintemp_c")]
        public double? MinimumTemperatureCelsius { get; set; }

        [JsonPropertyName("maxtemp_c")]
        public double? MaximumTemperatureCelsius { get; set; }

        [JsonPropertyName("daily_chance_of_rain")]
        public int? RainProbabilityPercent { get; set; }

        [JsonPropertyName("condition")]
        public WeatherCondition? Condition { get; set; }
    }

    private sealed class WeatherCondition
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}

public sealed class WeatherApiOptions
{
    public string ApiKey { get; set; } = string.Empty;
}

public sealed record WeatherSnapshot(
    string LocationName,
    DateTime? LastUpdatedLocal,
    double CurrentTemperatureCelsius,
    double CurrentFeelsLikeCelsius,
    string CurrentSummary,
    double? VisitMinimumTemperatureCelsius,
    double? VisitMaximumTemperatureCelsius,
    int? VisitPrecipitationProbabilityPercent,
    string? VisitSummary);
