using StackExchange.Redis;
using NRedisStack;
using System.Text.Json;

public class RedisCacheService
{
    private readonly IDatabase _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(ILogger<RedisCacheService> logger, ConnectionMultiplexer redis)
    {
        _logger = logger;
        _cache = redis.GetDatabase();

        _logger.LogInformation("RedisCacheService initialized");
    }

    public T GetOrSet<T>(string key, Func<T> getItemCallback, TimeSpan? expiry = null)
    {
        string jsonData = _cache.StringGet(key)!;
        if (!string.IsNullOrEmpty(jsonData))
        {
            return JsonSerializer.Deserialize<T>(jsonData)!;
        }

        T item = getItemCallback();
        _cache.StringSet(key, JsonSerializer.Serialize(item), expiry);
        return item;
    }
}
