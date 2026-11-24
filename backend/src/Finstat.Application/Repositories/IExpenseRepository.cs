using Finstat.Domain.Entities;

namespace Finstat.Application.Repositories;

public interface IExpenseRepository
{
    Task<ExpenseEntity?> GetAsync(Guid id, CancellationToken ct);
    Task<List<ExpenseEntity>> ListAsync(DateOnly fromDate, DateOnly toExclusive, CancellationToken ct);
    void Add(ExpenseEntity expense);
    void Remove(ExpenseEntity expense);
    Task SaveChangesAsync(CancellationToken ct);
}
