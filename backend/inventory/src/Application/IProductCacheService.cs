using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NovaCommerce.Inventory.Domain;

namespace NovaCommerce.Inventory.Application
{
    public interface IProductCacheService
    {
        Task<Product> GetProductAsync(Guid id);
        Task SetProductAsync(Product product);
        Task RemoveProductAsync(Guid id);
        Task<IEnumerable<Product>> GetProductsAsync();
        Task SetProductsAsync(IEnumerable<Product> products);
        Task RemoveProductsAsync();
    }
}
