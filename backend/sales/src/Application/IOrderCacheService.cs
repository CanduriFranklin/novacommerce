using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NovaCommerce.Sales.Domain;

namespace NovaCommerce.Sales.Application
{
    public interface IOrderCacheService
    {
        Task<Order> GetOrderAsync(Guid id);
        Task SetOrderAsync(Order order);
        Task RemoveOrderAsync(Guid id);
        Task<IEnumerable<Order>> GetOrdersAsync();
        Task SetOrdersAsync(IEnumerable<Order> orders);
        Task RemoveOrdersAsync();
    }
}
