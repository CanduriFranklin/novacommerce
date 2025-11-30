using Microsoft.EntityFrameworkCore;
using OutboxWorker.Infrastructure;

namespace OutboxWorker.Infrastructure
{
    public class OutboxDbContext : DbContext
    {
        public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options)
        {
        }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OccurredOn).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.ProcessedDate).IsRequired(false);
                entity.Property(e => e.RetryCount).IsRequired();
                entity.Property(e => e.Error).IsRequired(false);
            });
        }
    }
}
