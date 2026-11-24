using Finstat.Desktop.Core.DTOs;
using Finstat.Desktop.Core.ViewModels;

namespace Finstat.Desktop.Tests;

public class CategoryBreakdownTests
{
    [Fact]
    public void Categories_are_sorted_from_the_most_expensive()
    {
        var breakdown = CategoryBreakdown.Build(new List<CategoryStatisticsResponseDto>
        {
            Category("Транспорт", "365.50", "12.7"),
            Category("Їжа", "1623.15", "56.2"),
            Category("Розваги", "520.00", "18.0"),
        });

        Assert.Equal(new[] { "Їжа", "Розваги", "Транспорт" }, Names(breakdown.Rows));
        Assert.Equal(new[] { 0, 1, 2 }, ColorIndexes(breakdown.Segments));
    }

    [Fact]
    public void More_than_six_categories_are_folded_into_other_but_stay_in_the_table()
    {
        var categories = new List<CategoryStatisticsResponseDto>
        {
            Category("A", "400.00", "40.0"),
            Category("B", "200.00", "20.0"),
            Category("C", "150.00", "15.0"),
            Category("D", "100.00", "10.0"),
            Category("E", "80.00", "8.0"),
            Category("F", "40.00", "4.0"),
            Category("G", "30.00", "3.0"),
        };

        var breakdown = CategoryBreakdown.Build(categories);

        Assert.Equal(6, breakdown.Segments.Count);
        Assert.Equal("Інші", breakdown.Segments[5].Label);
        Assert.Equal(70, breakdown.Segments[5].Tenths);
        Assert.Equal(7, breakdown.Rows.Count);
        Assert.Equal(CategoryBreakdown.OtherKey, breakdown.Rows[6].SegmentKey);
    }

    [Fact]
    public void Zero_shares_get_no_segment()
    {
        var breakdown = CategoryBreakdown.Build(new List<CategoryStatisticsResponseDto>
        {
            Category("Їжа", "1000.00", "100.0"),
            Category("Таксі", "0.01", "0.0"),
        });

        Assert.Single(breakdown.Segments);
        Assert.Equal(2, breakdown.Rows.Count);
    }

    private static CategoryStatisticsResponseDto Category(string name, string total, string share)
    {
        return new CategoryStatisticsResponseDto(Guid.NewGuid(), name, false, total, share);
    }

    private static List<string> Names(List<BreakdownRow> rows)
    {
        var names = new List<string>();
        foreach (var row in rows)
            names.Add(row.Name);
        return names;
    }

    private static List<int> ColorIndexes(List<ChartSegment> segments)
    {
        var indexes = new List<int>();
        foreach (var segment in segments)
            indexes.Add(segment.ColorIndex);
        return indexes;
    }
}
