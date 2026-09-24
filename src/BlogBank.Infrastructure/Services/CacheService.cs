using BlogBank.Core.Enums;
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

    public async Task RemoveAsync(string key)
    {
        if (!IsEnabled) return;

        if (UseRedis)
        {
            try
            {
                var db = await _redis.GetDatabase().KeyDeleteAsync(key);
                return;
            }
            catch
            {
                // fall through to memory cache
            }
        }
        _memory.Remove(key);
    }

    public async Task SetAsync(string key, string value, int span, TimeEnum timeEnum)
    {
        if(!IsEnabled) return;
        var expiry = timeEnum switch
        {
            TimeEnum.Day    => TimeSpan.FromDays(span),
            TimeEnum.Hour   => TimeSpan.FromHours(span),
            TimeEnum.Minute => TimeSpan.FromMinutes(span),
            TimeEnum.Second => TimeSpan.FromSeconds(span),
            _ => throw new ArgumentOutOfRangeException(nameof(timeEnum), $"不支持的时间单位: {timeEnum}")
        };
        if (UseRedis)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, value, expiry);
            return;
        }
        _memory.Set(key, value, expiry);
    }

    public async Task ListRightPushAsync(string key, string value)
    {
        if(!IsEnabled) return;
        if (UseRedis)
        {
            try
            {
                var db = _redis.GetDatabase();
                await db.ListRightPushAsync(key, value);
            }
            catch
            {
                // fall through to memory cache
            }
        }
        _memory.Set(key, value);
    }

    public async Task<string> ListLeftPopAsync(string key)
    {
        if(!IsEnabled) return null;
        if (UseRedis)
        {
            try
            {
                var db = _redis.GetDatabase();
                await db.ListLeftPopAsync(key);
            }
            catch
            {
                // fall through to memory cache
            }
        }
        var res = _memory.Get<string>(key);
        return res;
    }

    public async Task<bool> IsExit(string key)
    {
        if(!IsEnabled) return false;
        if (UseRedis)
        {
            try
            {
                var db = _redis.GetDatabase();
                return await db.KeyExistsAsync(key);
            }
            catch
            {
                // fall through to memory cache
            }
        }
        var res = _memory.TryGetValue(key,out var obj);
        return res;
    }
    
    public async Task<long> Incr(string key)
    {
        if (UseRedis)
        {
            try
            {
                var db = _redis.GetDatabase();
                return await db.StringIncrementAsync(key);
            }
            catch (Exception e)
            {
                
                throw;
            }
        }

        return 1L;
    }

    // 加锁
    public async Task<bool> AcquireAsync(string key, string value, TimeSpan span)
    {
        if(!IsEnabled) return false;
        if (UseRedis)
        {
            return await _redis.GetDatabase().StringSetAsync(key, value, span, When.NotExists);
        }

        return false;
    }

    // 释放锁
    public async Task<bool> ReleaseAsync(string key, string value)
    {
        const string lua = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('DEL', KEYS[1])
            else
                return 0
            end";
        if(!IsEnabled) return false;
        if (UseRedis)
        {
            var n =  (int)await _redis.GetDatabase().ScriptEvaluateAsync(lua,
                keys: [key],
                values: [value]);
            return n == 1;
        }

        return false;
    }

    // 续签
    public async Task<bool> RenewAsync(string key, string value, TimeSpan span)
    {
        const string lua = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('EXPIRE', KEYS[1], ARGV[2])
            else
                return 0
            end";
        if(!IsEnabled) return false;
        if (UseRedis)
        {
            var n =  (int)await _redis.GetDatabase().ScriptEvaluateAsync(lua,
                keys: [key],
                values: [value,(int)span.TotalSeconds]);
            return n == 1;
        }

        return false;
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
