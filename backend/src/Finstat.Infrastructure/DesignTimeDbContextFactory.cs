using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure;

internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FinstatDbContext>
{
    public FinstatDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FinstatDbContext>()
            .UseNpgsql("Host=localhost;Database=finstat_design")
            .Options;
        return new FinstatDbContext(options);
    }
}
