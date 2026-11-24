using Finstat.Desktop.Core.DTOs;
using Finstat.Desktop.Core.Formatting;

namespace Finstat.Desktop.Core.ViewModels;

public record ChartSegment(string Key, string Label, int Tenths, int ColorIndex);

public class BreakdownRow(string name, bool isArchived, string amountText, string shareText, string segmentKey, int colorIndex, bool showDivider)
    : ViewModelBase
{
    private bool isActive;

    public string Name { get; } = name;

    public bool IsArchived { get; } = isArchived;

    public string AmountText { get; } = amountText;

    public string ShareText { get; } = shareText;

    public string SegmentKey { get; } = segmentKey;

    public int ColorIndex { get; } = colorIndex;

    public bool ShowDivider { get; } = showDivider;

    public bool HasSegment { get; set; }

    public bool IsActive
    {
        get => isActive;
        set => SetProperty(ref isActive, value);
    }
}

public class CategoryBreakdown
{
    public const int MaxSegments = 6;
    public const string OtherKey = "other";

    private CategoryBreakdown(List<ChartSegment> segments, List<BreakdownRow> rows)
    {
        Segments = segments;
        Rows = rows;
    }

    public List<ChartSegment> Segments { get; }

    public List<BreakdownRow> Rows { get; }

    public static List<CategoryStatisticsResponseDto> SortByTotal(List<CategoryStatisticsResponseDto> categories)
    {
        var sorted = new List<CategoryStatisticsResponseDto>(categories);
        sorted.Sort(CompareByTotal);
        return sorted;
    }

    public static CategoryBreakdown Build(List<CategoryStatisticsResponseDto> categories)
    {
        var sorted = SortByTotal(categories);
        var namedCount = sorted.Count;
        if (sorted.Count > MaxSegments)
            namedCount = MaxSegments - 1;

        var segments = new List<ChartSegment>();
        var rows = new List<BreakdownRow>();
        var otherTenths = 0;

        for (var index = 0; index < sorted.Count; index++)
        {
            var category = sorted[index];
            var tenths = MoneyFormatter.ShareInTenths(category.Share);
            var isNamed = index < namedCount;
            var key = OtherKey;
            var colorIndex = MaxSegments - 1;
            if (isNamed)
            {
                key = category.CategoryId.ToString();
                colorIndex = index;
                if (tenths > 0)
                    segments.Add(new ChartSegment(key, category.Name, tenths, colorIndex));
            }
            else
            {
                otherTenths += tenths;
            }

            rows.Add(new BreakdownRow(
                category.Name,
                category.IsArchived,
                MoneyFormatter.Format(category.Total),
                MoneyFormatter.FormatShare(category.Share),
                key,
                colorIndex,
                index > 0));
        }

        if (otherTenths > 0)
            segments.Add(new ChartSegment(OtherKey, "Інші", otherTenths, MaxSegments - 1));

        foreach (var row in rows)
        {
            foreach (var segment in segments)
            {
                if (segment.Key == row.SegmentKey)
                    row.HasSegment = true;
            }
        }

        return new CategoryBreakdown(segments, rows);
    }

    public static string TenthsToShare(int tenths)
    {
        return MoneyFormatter.FormatShare($"{tenths / 10}.{tenths % 10}");
    }

    private static int CompareByTotal(CategoryStatisticsResponseDto left, CategoryStatisticsResponseDto right)
    {
        var byTotal = MoneyFormatter.CompareAmounts(right.Total, left.Total);
        if (byTotal != 0)
            return byTotal;
        return string.Compare(left.Name, right.Name, StringComparison.CurrentCulture);
    }
}
