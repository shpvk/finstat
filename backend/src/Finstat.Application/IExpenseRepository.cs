using Finstat.Domain;

namespace Finstat.Application;

public interface IExpenseRepository
{
    Task<Expense?> GetAsync(Guid id, CancellationToken ct);
    Task<List<Expense>> ListAsync(DateOnly from, DateOnly toExclusive, CancellationToken ct);
    void Add(Expense expense);
    void Remove(Expense expense);
    Task SaveChangesAsync(CancellationToken ct);
}
