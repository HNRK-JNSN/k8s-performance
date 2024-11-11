using Microsoft.Extensions.Caching.Distributed;


public class WeatherForecastDTO
{
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}

public class WeatherforecastAPI
{
    private readonly ILogger<WeatherforecastAPI> _logger;
    private readonly IDistributedCache _cache;

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public WeatherforecastAPI(ILogger<WeatherforecastAPI> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public async Task<IResult> GetWeatherForecastAsync()
    {
        _logger.LogInformation("Getting weather forecast");

        // Attempt to get cached weather data
        var cacheKey = "weather_forecast";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Returning cached weather data.");
            var weatherForecasts = System.Text.Json.JsonSerializer.Deserialize<List<WeatherForecastDTO>>(cachedData);
            return Results.Ok(weatherForecasts);
        }

        // Generate new weather data if cache is empty
        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecastDTO
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToList();

        // Cache the data with a sliding expiration of 30 minutes
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };

        var serializedData = System.Text.Json.JsonSerializer.Serialize(forecasts);
        await _cache.SetStringAsync(cacheKey, serializedData, options);

        return Results.Ok(forecasts);
    }
}