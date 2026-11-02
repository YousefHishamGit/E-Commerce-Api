using Application.Contracts.Repository;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CacheRepository : ICacheRepository
    {
        private readonly IDatabase _database;

        public CacheRepository(IConnectionMultiplexer connection) {
            _database = connection.GetDatabase();
        }

        public async Task<string?> GetAsync(string key)
        {
            var value = await _database.StringGetAsync(key);
            return value;
        }

        public async Task SetAsync(string key, object value, TimeSpan duration)
        {
            await _database.StringSetAsync(key, JsonSerializer.Serialize(value), duration);
        }
        public async Task DeleteAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
