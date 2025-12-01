using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovaCommerce.Inventory.Domain;

namespace NovaCommerce.Inventory.Infrastructure
{
    public class ProductRepository : IProductRepository
    {
        private readonly InventoryDbContext _context;

        public ProductRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetByIdAsync(Guid id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            var maxRetries = 3;
            for (var i = 0; i < maxRetries; i++)
            {
                try
                {
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                    return;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    if (databaseValues == null)
                    {
                        throw new NotSupportedException("Cannot handle concurrency conflicts for deleted entities.");
                    }

                    var databaseEntry = databaseValues.ToObject() as Product;
                    // Refresh the original values to bypass the optimistic concurrency check
                    entry.OriginalValues.SetValues(databaseValues);
                }
            }
            throw new DbUpdateConcurrencyException("Failed to update product after multiple retries.");
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
