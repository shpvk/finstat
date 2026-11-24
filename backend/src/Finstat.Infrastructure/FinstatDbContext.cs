using Finstat.Domain;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure;

public class FinstatDbContext(DbContextOptions<FinstatDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinstatDbContext).Assembly);
    }
}
