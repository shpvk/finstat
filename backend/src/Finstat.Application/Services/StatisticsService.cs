using System.Globalization;
using Finstat.Application.DTOs;
using Finstat.Application.Queries;

namespace Finstat.Application.Services;

public class StatisticsService(IStatisticsQuery query)
{
    public const int RecentCount = 3;

    public async Task<MonthStatisticsResponseDto> GetAsync(DateOnly month, CancellationToken ct)
    {
        var from = new DateOnly(month.Year, month.Month, 1);
        var to = from.AddMonths(1);
        var totals = await query.CategoryTotalsAsync(from, to, ct);
        var recent = await query.RecentAsync(from, to, RecentCount, ct);

        var total = 0m;
        foreach (var categoryTotal in totals)
            total += categoryTotal.Total;

        var categories = new List<CategoryStatisticsResponseDto>();
        foreach (var categoryTotal in totals)
        {
            var share = Share(categoryTotal.Total, total);
            categories.Add(new CategoryStatisticsResponseDto(categoryTotal.CategoryId, categoryTotal.Name, categoryTotal.IsArchived, Format(categoryTotal.Total), share));
        }

        var recentExpenses = new List<ExpenseResponseDto>();
        foreach (var expense in recent)
            recentExpenses.Add(ExpenseService.ToDto(expense));

        var monthText = from.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        return new MonthStatisticsResponseDto(monthText, Format(total), categories, recentExpenses);
    }

    public static string Share(decimal part, decimal total)
    {
        if (total == 0)
            return "0.0";
        var percent = decimal.Round(part / total * 100, 1, MidpointRounding.AwayFromZero);
        return percent.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static string Format(decimal value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
