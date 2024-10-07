using MealSync.Application.Common.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace MealSync.Infrastructure.Services;

public class CacheService : ICacheService, IBaseService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public CacheService(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task SetCacheResponseAsync(string key, object? response, TimeSpan timeout)
    {
        if (response == null)
        {
            return;
        }

        var serializerResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        });
        await _distributedCache.SetStringAsync(key, serializerResponse, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeout,
        });
    }

    public async Task<string?> GetCachedResponseAsync(string key)
    {
        var cacheResponse = await _distributedCache.GetStringAsync(key);
        return string.IsNullOrEmpty(cacheResponse) ? null : cacheResponse;
    }

    public async Task RemoveCacheResponseAsync(string pattern)
    {
        foreach (var key in GetKeyAsync(pattern + "*"))
        {
            await _distributedCache.RemoveAsync(key);
        }
    }

    private IEnumerable<string> GetKeyAsync(string pattern)
    {
        foreach (var endPoint in _connectionMultiplexer.GetEndPoints())
        {
            var server = _connectionMultiplexer.GetServer(endPoint);
            foreach (var key in server.Keys(pattern: pattern))
            {
                yield return key.ToString();
            }
        }
    }

    public async Task<string> GenerateAndSetCodeToCacheAsync(string key, int secondsToExpire)
    {
        // Generate code an set to cache
        var code = new Random().Next(1000, 10000).ToString();
        // Set to cache
        await SetCacheResponseAsync(key, code, TimeSpan.FromSeconds(secondsToExpire))
            .ConfigureAwait(false);
        return code;
    }
}