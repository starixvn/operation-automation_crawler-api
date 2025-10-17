using Fanex.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace Helper;

public class DynamicCaching(IMemoryCache memoryCache, ICache redisCache)
{
    public bool TryGetValue<T>(string key, out T? value)
    {
        if (memoryCache.TryGetValue(key, out T memoryValue))
        {
            value = memoryValue;
            return true;
        }

        if (redisCache.TryGet(key, out T redisValue))
        {
            // Store in memory cache for faster access next time
            memoryCache.Set(key, redisValue);
            value = redisValue;
            return true;
        }

        value = default;

        return false;
    }

    public void SetValue<T>(string key, T value, TimeSpan cacheDuration)
    {
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(cacheDuration);
        memoryCache.Set(key, value, cacheOptions);

        var cacheItemOptions = new CacheItemOptions().SetAbsoluteExpiration(cacheDuration);
        
        redisCache.Set(key, value, cacheItemOptions);
    }
}