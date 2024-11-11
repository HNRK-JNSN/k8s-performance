using System.Diagnostics;
using NLog;
using OpenTelemetry.Metrics;
using StackExchange.Redis;
using NRedisStack;

var logger = NLog.LogManager.Setup().LoadConfigurationFromFile().GetCurrentClassLogger();
logger.Info("Starting Test Weather Service ...");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add OpenTelemetry services
builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.AddPrometheusExporter();

        builder.AddMeter("Microsoft.AspNetCore.Hosting",
                         "Microsoft.AspNetCore.Server.Kestrel");
        builder.AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                       0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
            });
    });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "TestServer_";
});

builder.Services.AddSingleton<WeatherforecastAPI>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPrometheusScrapingEndpoint();

app.MapGet("/", () => "Welcome to the Weatherforecast Service");

app.MapGet("/weatherforecast", async (WeatherforecastAPI api) => await api.GetWeatherForecastAsync())
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/telemetry", () => "OpenTelemetry! ticks:"
                     + DateTime.Now.Ticks.ToString()[^5..]);

app.MapGet("/version", () => new Dictionary<string, string>
{
    { "service", "Weather Forecast" },
    { "version", typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown" },
    { "ip-address", System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[0].MapToIPv4().ToString() }
});

logger.Info("WeatherForecast service started");

app.Run();
