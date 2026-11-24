using Finstat.Application;
using Finstat.Domain;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure.Statistics;

public class StatisticsQuery(FinstatDbContext db) : IStatisticsQuery
{
    public Task<List<CategoryTotal>> CategoryTotalsAsync(DateOnly from, DateOnly toExclusive, CancellationToken ct) =>
        db.Database.SqlQuery<CategoryTotal>($"""
                SELECT c."Id" AS "CategoryId", c."Name" AS "Name", c."IsArchived" AS "IsArchived", SUM(e."Amount") AS "Total"
                FROM "Expenses" e
                JOIN "Categories" c ON c."Id" = e."CategoryId"
                WHERE e."Date" >= {from} AND e."Date" < {toExclusive}
                GROUP BY c."Id", c."Name", c."IsArchived"
                ORDER BY "Total" DESC, c."Name"
                """)
            .ToListAsync(ct);

    public Task<List<Expense>> RecentAsync(DateOnly from, DateOnly toExclusive, int count, CancellationToken ct) =>
        db.Expenses.Where(e => e.Date >= from && e.Date < toExclusive)
            .OrderByDescending(e => e.Date).ThenByDescending(e => e.CreatedAt).Take(count).ToListAsync(ct);
}
