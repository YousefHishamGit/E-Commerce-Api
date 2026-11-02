using Application.Contracts.Services.Common;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Common
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache) {
            _cache=cache;
        }
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            try { 
                var value = await _cache.GetStringAsync(key);
                if(string.IsNullOrEmpty(value)) return null;

                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving cache for key: {key}", ex);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing cache for key: {key}", ex);
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            try {
                var data = JsonSerializer.Serialize(value);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };
                await _cache.SetStringAsync(key, data, options, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error setting cache for key: {key}", ex);
            }
        }
    }
}
