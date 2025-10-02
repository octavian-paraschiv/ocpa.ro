using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace ocpa.ro.domain.Abstractions.Services;

public interface ICacheService
{
    Task<T> ReadCachedData<T>(string key, Func<Task<T>> fallback = null) where T : class;
    Task<T> Read<T>(string key) where T : class;

    Task Save<T>(string key, T data) where T : class;
    Task Save<T>(string key, T data, DistributedCacheEntryOptions options) where T : class;
}
