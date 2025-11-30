using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaCommerce.Sales.Domain
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetAllAsync();
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
}
