using BlogBank.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace BlogBank.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IMemoryCache _memory;
    private readonly IConfiguration _config;

    public CacheService(IConnectionMultiplexer? redis, IMemoryCache memory, IConfiguration config)
    {
        _redis = redis;
        _memory = memory;
        _config = config;
    }

    public bool IsEnabled =>
        bool.TryParse(_config["Cache:Enabled"], out var enabled) && enabled;

    private bool UseRedis => IsEnabled && _redis is { IsConnected: true };

    public async Task<string?> GetAsync(string key)
    {
        if (!IsEnabled) return null;

        if (UseRedis)
        {
            try
            {
                var db = _redis.GetDatabase();
                var val = await db.StringGetAsync(key);
                return val.HasValue ? (string?)val : null;
            }
            catch
            {
                // fall through to memory cache
            }
        }

        _memory.TryGetValue(key, out string? cached);
        return cached;
    }

    public async Task SetAsync(string key, string value, string resource = "default")
    {
        if (!IsEnabled) return;

        var expiry = TimeSpan.FromSeconds(GetExpiry(resource));

        if (UseRedis)
        {
            try
            {
                var db = _redis.GetDatabase();
                await db.StringSetAsync(key, value, expiry);
                return;
            }
            catch
            {
                // fall through to memory cache
            }
        }

        _memory.Set(key, value, expiry);
    }

    public async Task RemoveAsync(params string[] keys)
    {
        if (!IsEnabled) return;

        if (UseRedis)
        {
            try
            {
                var db = _redis.GetDatabase();
                foreach (var key in keys)
                    await db.KeyDeleteAsync(key);
                return;
            }
            catch
            {
                // fall through to memory cache
            }
        }

        foreach (var key in keys)
            _memory.Remove(key);
    }

    private int GetExpiry(string resource)
    {
        var specific = _config[$"Cache:{resource}"];
        if (specific != null && int.TryParse(specific, out var val) && val > 0)
            return val;

        var defaultVal = _config["Cache:DefaultExpirySeconds"];
        return defaultVal != null && int.TryParse(defaultVal, out var def) ? def : 300;
    }
}
