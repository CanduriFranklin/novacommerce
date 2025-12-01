using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NovaCommerce.Sales.Domain;
using StackExchange.Redis;

namespace NovaCommerce.Sales.Application
{
    public class RedisOrderCacheService : IOrderCacheService
    {
        private readonly IDatabase _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private const string AllOrdersCacheKey = "AllOrders";

        public RedisOrderCacheService(IConnectionMultiplexer redis)
        {
            _cache = redis.GetDatabase();
        }

        public async Task<Order> GetOrderAsync(Guid id)
        {
            var cachedOrder = await _cache.StringGetAsync(id.ToString());
            return cachedOrder.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Order>(cachedOrder);
        }

        public async Task SetOrderAsync(Order order)
        {
            await _cache.StringSetAsync(order.OrderId.ToString(), JsonSerializer.Serialize(order), _cacheDuration);
            await RemoveOrdersAsync(); // Invalidate all orders cache
        }

        public async Task RemoveOrderAsync(Guid id)
        {
            await _cache.KeyDeleteAsync(id.ToString());
            await RemoveOrdersAsync(); // Invalidate all orders cache
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            var cachedOrders = await _cache.StringGetAsync(AllOrdersCacheKey);
            return cachedOrders.IsNullOrEmpty ? null : JsonSerializer.Deserialize<IEnumerable<Order>>(cachedOrders);
        }

        public async Task SetOrdersAsync(IEnumerable<Order> orders)
        {
            await _cache.StringSetAsync(AllOrdersCacheKey, JsonSerializer.Serialize(orders), _cacheDuration);
        }

        public async Task RemoveOrdersAsync()
        {
            await _cache.KeyDeleteAsync(AllOrdersCacheKey);
        }
    }
}
