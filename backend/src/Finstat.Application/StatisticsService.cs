using System.Globalization;

namespace Finstat.Application;

public record CategoryStatisticsDto(Guid CategoryId, string Name, bool IsArchived, string Total, string Share);

public record MonthStatisticsDto(string Month, string Total, List<CategoryStatisticsDto> Categories, List<ExpenseDto> Recent);

public class StatisticsService(IStatisticsQuery query)
{
    public const int RecentCount = 3;

    public async Task<MonthStatisticsDto> GetAsync(DateOnly month, CancellationToken ct)
    {
        var from = new DateOnly(month.Year, month.Month, 1);
        var to = from.AddMonths(1);
        var totals = await query.CategoryTotalsAsync(from, to, ct);
        var recent = await query.RecentAsync(from, to, RecentCount, ct);
        var total = totals.Sum(c => c.Total);
        return new MonthStatisticsDto(
            from.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            Format(total),
            totals.Select(c => new CategoryStatisticsDto(c.CategoryId, c.Name, c.IsArchived, Format(c.Total), Share(c.Total, total))).ToList(),
            recent.Select(ExpenseService.ToDto).ToList());
    }

    public static string Share(decimal part, decimal total) =>
        total == 0 ? "0.0" : decimal.Round(part / total * 100, 1, MidpointRounding.AwayFromZero).ToString("0.0", CultureInfo.InvariantCulture);

    private static string Format(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
