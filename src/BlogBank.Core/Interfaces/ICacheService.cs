namespace BlogBank.Core.Interfaces;

public interface ICacheService
{
    bool IsEnabled { get; }
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, string resource = "default");
    Task RemoveAsync(params string[] keys);
}
