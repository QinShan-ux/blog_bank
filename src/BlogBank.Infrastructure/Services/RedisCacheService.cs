using BlogBank.Core.Enums;
using BlogBank.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace BlogBank.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IMemoryCache _memory;  // 加上内存缓存兜底
    private readonly IConfiguration _config;
    private readonly bool _useRedis;

    public RedisCacheService(
        IConnectionMultiplexer? redis,
        IMemoryCache memory,  // 注入内存缓存
        IConfiguration config)
    {
        _redis = redis;
        _memory = memory;
        _config = config;
        _useRedis = redis is { IsConnected: true };
    }

    public bool IsEnabled => true;

    // ========== 读 ==========

    public async Task<string?> GetAsync(string key)
    {
        if (!IsEnabled) return null;

        // 1. 先读内存
        if (_memory.TryGetValue(key, out string? memVal))
            return memVal;

        // 2. 再读 Redis
        if (_useRedis)
        {
            try
            {
                var db = _redis!.GetDatabase();
                var val = await db.StringGetAsync(key);
                if (val.HasValue)
                {
                    // 回填内存
                    _memory.Set(key, (string?)val, TimeSpan.FromMinutes(5));
                    return val;
                }
            }
            catch
            {
                // Redis 失败，继续走内存逻辑
            }
        }

        return null;
    }

    // ========== 写 ==========

    public async Task SetAsync(string key, string value, string resource = "default")
    {
        if (!IsEnabled) return;

        var expiry = TimeSpan.FromSeconds(GetExpiry(resource));

        // 写内存
        _memory.Set(key, value, expiry);

        // 写 Redis
        if (_useRedis)
        {
            try
            {
                var db = _redis!.GetDatabase();
                await db.StringSetAsync(key, value, expiry);
            }
            catch
            {
                // Redis 失败，内存已有数据
            }
        }
    }

    public async Task SetAsync(string key, string value, int span, TimeEnum timeEnum)
    {
        if (!IsEnabled) return;

        var expiry = timeEnum switch
        {
            TimeEnum.Day => TimeSpan.FromDays(span),
            TimeEnum.Hour => TimeSpan.FromHours(span),
            TimeEnum.Minute => TimeSpan.FromMinutes(span),
            TimeEnum.Second => TimeSpan.FromSeconds(span),
            _ => throw new ArgumentOutOfRangeException(nameof(timeEnum), $"不支持的时间单位: {timeEnum}")
        };

        // 写内存
        _memory.Set(key, value, expiry);

        if (_useRedis)
        {
            try
            {
                var db = _redis!.GetDatabase();
                await db.StringSetAsync(key, value, expiry);
            }
            catch { }
        }
    }

    // ========== 删 ==========

    public async Task RemoveAsync(params string[] keys)
    {
        if (!IsEnabled) return;

        // 删内存
        foreach (var key in keys)
            _memory.Remove(key);

        if (_useRedis)
        {
            try
            {
                var db = _redis!.GetDatabase();
                foreach (var key in keys)
                    await db.KeyDeleteAsync(key);
            }
            catch { }
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (!IsEnabled) return;

        // 删内存
        _memory.Remove(key);

        if (_useRedis)
        {
            try
            {
                var db = _redis!.GetDatabase();
                await db.KeyDeleteAsync(key);
            }
            catch { }
        }
    }

    // ========== 列表操作 ==========

    public async Task ListRightPushAsync(string key, string value)
    {
        if (!IsEnabled) return;

        if (_useRedis)
        {
            try
            {
                var db = _redis!.GetDatabase();
                await db.ListRightPushAsync(key, value);
            }
            catch { }
        }
    }

    public async Task<string?> ListLeftPopAsync(string key)
    {
        if (!IsEnabled) return null;

        if (_useRedis)
        {
            try
            {
                var db = _redis!.GetDatabase();
                var result = await db.ListLeftPopAsync(key);
                return result.HasValue ? (string?)result : null;
            }
            catch { }
        }

        return null;
    }

    public async Task<bool> IsExit(string key)
    {
        throw new NotImplementedException();
    }

    // ========== 其他 ==========

    public async Task<bool> Exists(string key)  // 改名 IsExit -> Exists
    {
        if (!IsEnabled) return false;

        // 先查内存
        if (_memory.TryGetValue(key, out _)) return true;

        if (_useRedis)
        {
            try
            {
                var db = _redis!.GetDatabase();
                return await db.KeyExistsAsync(key);
            }
            catch { }
        }

        return false;
    }

    public async Task<long> Incr(string key)
    {
        if (_useRedis)
        {
            try
            {
                var db = _redis!.GetDatabase();
                return await db.StringIncrementAsync(key);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        // 内存版本（简单模拟）
        var val = _memory.GetOrCreate(key, _ => 0L);
        val++;
        _memory.Set(key, val);
        return val;
    }

    // ========== 分布式锁 ==========

    public async Task<bool> AcquireAsync(string key, string value, TimeSpan span)
    {
        if (!IsEnabled) return false;

        if (_useRedis)
        {
            try
            {
                return await _redis!.GetDatabase().StringSetAsync(key, value, span, When.NotExists);
            }
            catch { }
        }

        // 内存锁兜底
        if (_memory.TryGetValue(key, out _)) return false;
        _memory.Set(key, value, span);
        return true;
    }

    public async Task<bool> ReleaseAsync(string key, string value)
    {
        const string lua = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('DEL', KEYS[1])
            else
                return 0
            end";

        if (!IsEnabled) return false;

        // 先删内存
        _memory.Remove(key);

        if (_useRedis)
        {
            try
            {
                var n = (int)await _redis!.GetDatabase().ScriptEvaluateAsync(lua,
                    keys: [key],
                    values: [value]);
                return n == 1;
            }
            catch { }
        }

        return true;
    }

    public async Task<bool> RenewAsync(string key, string value, TimeSpan span)
    {
        const string lua = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('EXPIRE', KEYS[1], ARGV[2])
            else
                return 0
            end";

        if (!IsEnabled) return false;

        // 续签内存
        if (_memory.TryGetValue(key, out var oldVal) && (string?)oldVal == value)
            _memory.Set(key, value, span);

        if (_useRedis)
        {
            try
            {
                var n = (int)await _redis!.GetDatabase().ScriptEvaluateAsync(lua,
                    keys: [key],
                    values: [value, (int)span.TotalSeconds]);
                return n == 1;
            }
            catch { }
        }

        return false;
    }

    // ========== 辅助 ==========

    private int GetExpiry(string resource)
    {
        var specific = _config[$"Cache:{resource}"];
        if (specific != null && int.TryParse(specific, out var val) && val > 0)
            return val;

        var defaultVal = _config["Cache:DefaultExpirySeconds"];
        return defaultVal != null && int.TryParse(defaultVal, out var def) ? def : 300;
    }
}