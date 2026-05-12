using BlogBank.Core.Enums;

namespace BlogBank.Core.Interfaces;

public interface ICacheService
{
    bool IsEnabled { get; }
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, string resource = "default");
    Task RemoveAsync(params string[] keys);

    Task SetAsync(string key, string value, int span, TimeEnum timeEnum);

    Task ListRightPushAsync(string key, string value);

    Task<string>ListLeftPopAsync(string key);
}
