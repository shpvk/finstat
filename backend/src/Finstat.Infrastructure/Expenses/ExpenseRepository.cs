using Finstat.Application;
using Finstat.Domain;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure.Expenses;

public class ExpenseRepository(FinstatDbContext db) : IExpenseRepository
{
    public Task<Expense?> GetAsync(Guid id, CancellationToken ct) =>
        db.Expenses.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<List<Expense>> ListAsync(DateOnly from, DateOnly toExclusive, CancellationToken ct) =>
        db.Expenses.Where(e => e.Date >= from && e.Date < toExclusive)
            .OrderByDescending(e => e.Date).ThenByDescending(e => e.CreatedAt).ToListAsync(ct);

    public void Add(Expense expense) => db.Expenses.Add(expense);

    public void Remove(Expense expense) => db.Expenses.Remove(expense);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
