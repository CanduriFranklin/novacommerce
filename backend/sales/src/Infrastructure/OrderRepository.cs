using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovaCommerce.Sales.Domain;

namespace NovaCommerce.Sales.Infrastructure
{
    public class OrderRepository : IOrderRepository
    {
        private readonly SalesDbContext _context;

        public OrderRepository(SalesDbContext context)
        {
            _context = context;
        }

        public async Task<Order> GetByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ToListAsync();
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
