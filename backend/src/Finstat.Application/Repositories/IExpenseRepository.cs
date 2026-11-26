using Finstat.Application.DTOs;
using Finstat.Domain.Entities;

namespace Finstat.Application.Repositories;

public interface IExpenseRepository
{
    Task<ExpenseEntity?> GetAsync(Guid id, CancellationToken ct);
    Task<List<ExpenseEntity>> ListAsync(DateOnly fromDate, DateOnly toExclusive, CancellationToken ct);
    Task<List<ExpenseEntity>> RecentAsync(DateOnly fromDate, DateOnly toExclusive, int count, CancellationToken ct);
    Task<List<CategoryTotalDto>> CategoryTotalsAsync(DateOnly fromDate, DateOnly toExclusive, CancellationToken ct);
    void Add(ExpenseEntity expense);
    void Remove(ExpenseEntity expense);
    Task SaveChangesAsync(CancellationToken ct);
}
