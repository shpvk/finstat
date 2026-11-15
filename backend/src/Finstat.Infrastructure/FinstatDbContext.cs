using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure;

public class FinstatDbContext(DbContextOptions<FinstatDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinstatDbContext).Assembly);
}
