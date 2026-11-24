using Finstat.Domain;

namespace Finstat.Application;

public record CategoryTotal(Guid CategoryId, string Name, bool IsArchived, decimal Total);

public interface IStatisticsQuery
{
    Task<List<CategoryTotal>> CategoryTotalsAsync(DateOnly from, DateOnly toExclusive, CancellationToken ct);
    Task<List<Expense>> RecentAsync(DateOnly from, DateOnly toExclusive, int count, CancellationToken ct);
}
