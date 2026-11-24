using Finstat.Application.Repositories;
using Finstat.Domain.Entities;
using Finstat.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure.Repositories;

public class ExpenseRepository(FinstatDbContext db) : IExpenseRepository
{
    public async Task<ExpenseEntity?> GetAsync(Guid id, CancellationToken ct)
    {
        var query = from expense in db.Expenses
                    where expense.Id == id
                    select expense;
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<List<ExpenseEntity>> ListAsync(DateOnly fromDate, DateOnly toExclusive, CancellationToken ct)
    {
        var query = from expense in db.Expenses
                    where expense.Date >= fromDate && expense.Date < toExclusive
                    orderby expense.Date descending, expense.CreatedAt descending
                    select expense;
        return await query.ToListAsync(ct);
    }

    public void Add(ExpenseEntity expense)
    {
        db.Expenses.Add(expense);
    }

    public void Remove(ExpenseEntity expense)
    {
        db.Expenses.Remove(expense);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
