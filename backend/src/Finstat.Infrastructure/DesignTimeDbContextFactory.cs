using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Finstat.Infrastructure;

internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FinstatDbContext>
{
    public FinstatDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<FinstatDbContext>().UseNpgsql("Host=localhost;Database=finstat_design").Options);
}
