using Microsoft.EntityFrameworkCore;
using NovaCommerce.Inventory.Domain;

namespace NovaCommerce.Inventory.Infrastructure
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProcessedEvent> ProcessedEvents { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Trace> Traces { get; set; } // Added Trace DbSet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<ProcessedEvent>()
                .HasKey(pe => new { pe.OrderId, pe.EventType });
        }
    }
}
