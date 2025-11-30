using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NovaCommerce.Inventory.Domain;
using StackExchange.Redis;

namespace NovaCommerce.Inventory.Application
{
    public class RedisProductCacheService : IProductCacheService
    {
        private readonly IDatabase _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private const string AllProductsCacheKey = "AllProducts";

        public RedisProductCacheService(IConnectionMultiplexer redis)
        {
            _cache = redis.GetDatabase();
        }

        public async Task<Product> GetProductAsync(Guid id)
        {
            var cachedProduct = await _cache.StringGetAsync(id.ToString());
            return cachedProduct.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Product>(cachedProduct);
        }

        public async Task SetProductAsync(Product product)
        {
            await _cache.StringSetAsync(product.ProductId.ToString(), JsonSerializer.Serialize(product), _cacheDuration);
            await RemoveProductsAsync(); // Invalidate all products cache
        }

        public async Task RemoveProductAsync(Guid id)
        {
            await _cache.KeyDeleteAsync(id.ToString());
            await RemoveProductsAsync(); // Invalidate all products cache
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            var cachedProducts = await _cache.StringGetAsync(AllProductsCacheKey);
            return cachedProducts.IsNullOrEmpty ? null : JsonSerializer.Deserialize<IEnumerable<Product>>(cachedProducts);
        }

        public async Task SetProductsAsync(IEnumerable<Product> products)
        {
            await _cache.StringSetAsync(AllProductsCacheKey, JsonSerializer.Serialize(products), _cacheDuration);
        }

        public async Task RemoveProductsAsync()
        {
            await _cache.KeyDeleteAsync(AllProductsCacheKey);
        }
    }
}
