using Microsoft.Extensions.Caching.Distributed;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Models.Configuration;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ocpa.ro.application.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly CacheConfig _cacheSettings;

    public CacheService(IDistributedCache cache, ISystemSettingsService systemSettings)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cacheSettings = systemSettings.CacheSettings;
    }

    async Task<T> ICacheService.ReadCachedData<T>(string key, Func<Task<T>> fallback, DistributedCacheEntryOptions options) where T : class
    {
        var t = await (this as ICacheService).Read<T>(key);

        if (t == default)
            t = await fallback?.Invoke();

        await (this as ICacheService).Save(key, t, options);

        return t;
    }

    async Task<T> ICacheService.Read<T>(string key) where T : class
    {
        var str = await _cache.GetStringAsync(key);
        return string.IsNullOrEmpty(str) ? default : JsonSerializer.Deserialize<T>(str);
    }

    Task ICacheService.Save<T>(string key, T data) where T : class
        => (this as ICacheService).Save<T>(key, data, null);

    async Task ICacheService.Save<T>(string key, T data, DistributedCacheEntryOptions options) where T : class
    {
        if (data == default)
            await _cache.RemoveAsync(key);
        else
        {
            var str = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(key, str, options ?? new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.CachePeriod)
            });
        }
    }
}
