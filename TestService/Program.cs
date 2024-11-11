using System.Diagnostics;
using NLog;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using StackExchange.Redis;
using NRedisStack;

var logger = NLog.LogManager.Setup().LoadConfigurationFromFile().GetCurrentClassLogger();
logger.Info("Starting Test Weather Service ...");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Create a service to expose ActivitySource, and Metric Instruments
// for manual instrumentation
builder.Services.AddSingleton<Instrumentation>();

// Add OpenTelemetry services
builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.AddPrometheusExporter();
        
        builder.AddMeter(Instrumentation.MeterName);

        builder.AddMeter("Microsoft.AspNetCore.Hosting",
                         "Microsoft.AspNetCore.Server.Kestrel");
        builder.AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                       0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
            });       

    });

var redisendpoint = builder.Configuration["RedisConnectionString"] ?? "localhost:6379";
logger.Info("Using Redis adress: {0}", redisendpoint);

var password = builder.Configuration["REDISPASSWORD"] ?? "";

builder.Services.AddStackExchangeRedisCache(options =>
{
    var configOptions = new ConfigurationOptions
    {
        EndPoints = { redisendpoint },
        Password = password,
        ConnectTimeout = 5000,
        SyncTimeout = 5000
    };   
    options.ConfigurationOptions = configOptions;
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
    { "file version", FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion ?? "unknown" },
    { "product version", FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion ?? "unknown" },
    { "ip-address", System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[0].MapToIPv4().ToString() }
});

logger.Info("WeatherForecast service started");

app.Run();
