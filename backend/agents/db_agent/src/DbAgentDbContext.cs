using Microsoft.EntityFrameworkCore;

public class DbAgentDbContext : DbContext
{
    public DbAgentDbContext(DbContextOptions<DbAgentDbContext> options) : base(options) { }
}
