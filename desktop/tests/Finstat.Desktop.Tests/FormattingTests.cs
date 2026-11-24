using Finstat.Desktop.Core.Formatting;

namespace Finstat.Desktop.Tests;

public class FormattingTests
{
    [Theory]
    [InlineData("1250.5", "1 250,50 грн")]
    [InlineData("9999999999.99", "9 999 999 999,99 грн")]
    [InlineData("0.1", "0,10 грн")]
    public void Money_is_formatted_in_ukrainian(string amount, string expected)
    {
        Assert.Equal(expected, Normalize(MoneyFormatter.Format(amount)));
    }

    [Theory]
    [InlineData("77.5", "77,5%")]
    [InlineData("100.0", "100,0%")]
    public void Share_is_formatted_without_space_before_percent(string share, string expected)
    {
        Assert.Equal(expected, MoneyFormatter.FormatShare(share));
    }

    [Theory]
    [InlineData(" 1 250,5 ", "1250.5")]
    [InlineData("380", "380")]
    [InlineData("007.10", "7.10")]
    public void Valid_amounts_are_normalized(string input, string expected)
    {
        var result = AmountParser.Parse(input);
        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Amount);
    }

    [Theory]
    [InlineData("", "Вкажіть суму.")]
    [InlineData("12,345", "Сума може мати не більше двох знаків після коми.")]
    [InlineData("0,00", "Сума має бути більшою за нуль.")]
    [InlineData("-5", "Вкажіть суму числом, наприклад 125,50.")]
    [InlineData("1e3", "Вкажіть суму числом, наприклад 125,50.")]
    [InlineData("12345678901", "Сума завелика.")]
    public void Invalid_amounts_are_rejected_instead_of_rounded(string input, string error)
    {
        var result = AmountParser.Parse(input);
        Assert.False(result.IsValid);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Months_and_days_are_written_in_ukrainian()
    {
        Assert.Equal("Листопад 2026", DateFormatter.FormatMonth(new DateOnly(2026, 11, 1)));
        Assert.Equal("листопад 2026", DateFormatter.FormatMonthLower(new DateOnly(2026, 11, 1)));
        Assert.Equal("24 листопада", DateFormatter.FormatDay(new DateOnly(2026, 11, 24)));
        Assert.Equal("вт", DateFormatter.FormatWeekday(new DateOnly(2026, 11, 24)));
        Assert.Equal("2026-01", DateFormatter.ToApiMonth(new DateOnly(2026, 1, 31)));
    }

    [Theory]
    [InlineData(1, "витрата")]
    [InlineData(3, "витрати")]
    [InlineData(5, "витрат")]
    [InlineData(11, "витрат")]
    [InlineData(22, "витрати")]
    public void Ukrainian_plural_forms_are_chosen_by_count(int count, string expected)
    {
        Assert.Equal(expected, UkrainianPlural.Choose(count, "витрата", "витрати", "витрат"));
    }

    private static string Normalize(string text)
    {
        return text.Replace(' ', ' ').Replace(' ', ' ');
    }
}
