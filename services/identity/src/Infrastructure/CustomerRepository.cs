using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovaCommerce.Identity.Domain;

namespace NovaCommerce.Identity.Infrastructure
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IdentityDbContext _context;

        public CustomerRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> GetByIdAsync(Guid id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer> GetByEmailAsync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }
    }
}
