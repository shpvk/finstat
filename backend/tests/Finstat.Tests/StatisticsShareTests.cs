using Finstat.Application;

namespace Finstat.Tests;

public class StatisticsShareTests
{
    [Theory]
    [InlineData("1", "3", "33.3")]
    [InlineData("2", "3", "66.7")]
    [InlineData("1", "8", "12.5")]
    [InlineData("1.25", "100", "1.3")]
    [InlineData("1", "1", "100.0")]
    [InlineData("0", "0", "0.0")]
    public void Share_is_rounded_to_one_decimal_away_from_zero(string part, string total, string expected) =>
        Assert.Equal(expected, StatisticsService.Share(decimal.Parse(part), decimal.Parse(total)));
}
