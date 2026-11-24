using Finstat.Application.DTOs;
using Finstat.Application.Queries;
using Finstat.Domain.Entities;
using Finstat.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure.Queries;

public class StatisticsQuery(FinstatDbContext db) : IStatisticsQuery
{
    public async Task<List<CategoryTotalDto>> CategoryTotalsAsync(DateOnly fromDate, DateOnly toExclusive, CancellationToken ct)
    {
        var query = db.Database.SqlQuery<CategoryTotalDto>($"""
                SELECT c."Id" AS "CategoryId", c."Name" AS "Name", c."IsArchived" AS "IsArchived", SUM(e."Amount") AS "Total"
                FROM "Expenses" e
                JOIN "Categories" c ON c."Id" = e."CategoryId"
                WHERE e."Date" >= {fromDate} AND e."Date" < {toExclusive}
                GROUP BY c."Id", c."Name", c."IsArchived"
                ORDER BY "Total" DESC, c."Name"
                """);
        return await query.ToListAsync(ct);
    }

    public async Task<List<ExpenseEntity>> RecentAsync(DateOnly fromDate, DateOnly toExclusive, int count, CancellationToken ct)
    {
        var query = from expense in db.Expenses
                    where expense.Date >= fromDate && expense.Date < toExclusive
                    orderby expense.Date descending, expense.CreatedAt descending
                    select expense;
        return await query.Take(count).ToListAsync(ct);
    }
}
