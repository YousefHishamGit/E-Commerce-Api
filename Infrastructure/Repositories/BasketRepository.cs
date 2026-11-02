using Application.Contracts.Repository;
using Domain.Entities;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDatabase _database;

        public BasketRepository(IConnectionMultiplexer connection) { 
            _database = connection.GetDatabase();
        }
        public async Task<CustomerBasket?> CreateOrUpdateBasketAsync(CustomerBasket basket, TimeSpan duration)
        {
            var flag = await _database.StringSetAsync(basket.Id,JsonSerializer.Serialize(basket),duration);
            if (!flag) {
                return null;
            }
            return basket;
        }

        public async Task<bool> DeleteBasketAsync(string basketId)
        {
           return await _database.KeyDeleteAsync(basketId);
        }

        public async Task<CustomerBasket?> GetBasketAsync(string basketId)
        {
            var value = await _database.StringGetAsync(basketId);
            if (value.IsNullOrEmpty)
            {
                return null; 
            }
            var basket = JsonSerializer.Deserialize<CustomerBasket>(value);
            if(basket is null)
            {
                return null;
            }
            return basket;
        }
    }
}
