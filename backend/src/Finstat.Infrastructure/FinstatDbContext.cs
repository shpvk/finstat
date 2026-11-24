using Finstat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure;

public class FinstatDbContext(DbContextOptions<FinstatDbContext> options) : DbContext(options)
{
    public DbSet<CategoryEntity> Categories
    {
        get { return Set<CategoryEntity>(); }
    }

    public DbSet<ExpenseEntity> Expenses
    {
        get { return Set<ExpenseEntity>(); }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinstatDbContext).Assembly);
    }
}
