using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPApplication.Interfaces;

namespace TMPInfrastructure.Implementations
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _cache;

        public CacheService(IConnectionMultiplexer redis)
        {
            _cache = redis.GetDatabase();
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var cachedData = await _cache.StringGetAsync(key);
            if (!cachedData.HasValue)
                return default;

            return JsonConvert.DeserializeObject<T>(cachedData);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var jsonData = JsonConvert.SerializeObject(value);
            await _cache.StringSetAsync(key, jsonData, expiry);
        }

        public async Task DeleteKeyAsync(string key)
        {
            await _cache.KeyDeleteAsync(key);
        }
    }
}
