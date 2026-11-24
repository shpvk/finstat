using Finstat.Domain;

namespace Finstat.Tests;

public class ExpenseTests
{
    [Theory]
    [InlineData("0.01", "0.01")]
    [InlineData("12.5", "12.50")]
    [InlineData("12.50", "12.50")]
    [InlineData("100", "100.00")]
    [InlineData("9999999999.99", "9999999999.99")]
    public void Valid_amount_is_kept_exactly(string input, string expected) =>
        Assert.Equal(expected, Money.Parse(input).ToString());

    [Theory]
    [InlineData("0")]
    [InlineData("0.00")]
    [InlineData("-1")]
    [InlineData("12.345")]
    [InlineData("0.001")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("1e2")]
    [InlineData("1,5")]
    [InlineData("10000000000")]
    public void Invalid_amount_is_rejected(string? input) =>
        Assert.Throws<DomainException>(() => Money.Parse(input));

    [Fact]
    public void Comment_is_trimmed_blank_becomes_null_and_length_is_limited()
    {
        var amount = new Money(1m);
        var date = new DateOnly(2026, 1, 1);
        Assert.Equal("a", new Expense(Guid.NewGuid(), amount, Guid.NewGuid(), date, " a ", DateTime.UtcNow).Comment);
        Assert.Null(new Expense(Guid.NewGuid(), amount, Guid.NewGuid(), date, "  ", DateTime.UtcNow).Comment);
        Assert.Throws<DomainException>(() => new Expense(Guid.NewGuid(), amount, Guid.NewGuid(), date, new string('x', 201), DateTime.UtcNow));
    }

    [Fact]
    public void Failed_change_leaves_expense_untouched()
    {
        var expense = new Expense(Guid.NewGuid(), new Money(5m), Guid.NewGuid(), new DateOnly(2026, 1, 1), "old", DateTime.UtcNow);
        Assert.Throws<DomainException>(() => expense.Change(new Money(7m), Guid.NewGuid(), new DateOnly(2026, 2, 2), new string('x', 201)));
        Assert.Equal("5.00", expense.Amount.ToString());
        Assert.Equal("old", expense.Comment);
    }

    [Fact]
    public void Default_money_is_rejected_by_expense() =>
        Assert.Throws<DomainException>(() => new Expense(Guid.NewGuid(), default, Guid.NewGuid(), new DateOnly(2026, 1, 1), null, DateTime.UtcNow));
}
