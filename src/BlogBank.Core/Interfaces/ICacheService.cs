using BlogBank.Core.Enums;
using Microsoft.Extensions.Primitives;

namespace BlogBank.Core.Interfaces;

public interface ICacheService
{
    bool IsEnabled { get; }
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, string resource = "default");
    Task RemoveAsync(params string[] keys);

    Task RemoveAsync(string key);

    Task SetAsync(string key, string value, int span, TimeEnum timeEnum);

    Task ListRightPushAsync(string key, string value);

    Task<string>ListLeftPopAsync(string key);

    Task<bool> IsExit(string key);

    Task<bool> AcquireAsync(string key,string value, TimeSpan span);

    Task<bool> ReleaseAsync(string key, string value);

    Task<bool> RenewAsync(string key, string value, TimeSpan span);

    Task<long> Incr(string key);
}
