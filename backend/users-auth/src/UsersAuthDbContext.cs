using Microsoft.EntityFrameworkCore;

public class UsersAuthDbContext : DbContext
{
    public UsersAuthDbContext(DbContextOptions<UsersAuthDbContext> options) : base(options) { }
}
