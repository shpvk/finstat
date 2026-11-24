using Finstat.Application.DTOs;
using Finstat.Domain.Entities;

namespace Finstat.Application.Queries;

public interface IStatisticsQuery
{
    Task<List<CategoryTotalDto>> CategoryTotalsAsync(DateOnly fromDate, DateOnly toExclusive, CancellationToken ct);
    Task<List<ExpenseEntity>> RecentAsync(DateOnly fromDate, DateOnly toExclusive, int count, CancellationToken ct);
}
