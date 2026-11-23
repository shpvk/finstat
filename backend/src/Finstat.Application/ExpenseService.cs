using Finstat.Domain;

namespace Finstat.Application;

public record ExpenseDto(Guid Id, string Amount, Guid CategoryId, DateOnly Date, string? Comment);

public class ExpenseService(IExpenseRepository expenses, ICategoryRepository categories)
{
    public async Task<List<ExpenseDto>> ListAsync(DateOnly month, CancellationToken ct)
    {
        var from = new DateOnly(month.Year, month.Month, 1);
        return (await expenses.ListAsync(from, from.AddMonths(1), ct)).Select(ToDto).ToList();
    }

    public async Task<ExpenseDto> CreateAsync(string? amount, Guid categoryId, DateOnly? date, string? comment, CancellationToken ct)
    {
        var money = Money.Parse(amount);
        await EnsureActiveCategoryAsync(categoryId, ct);
        var expense = new Expense(Guid.NewGuid(), money, categoryId, RequireDate(date), comment, DateTime.UtcNow);
        expenses.Add(expense);
        await expenses.SaveChangesAsync(ct);
        return ToDto(expense);
    }

    public async Task<ExpenseDto?> UpdateAsync(Guid id, string? amount, Guid categoryId, DateOnly? date, string? comment, CancellationToken ct)
    {
        var expense = await expenses.GetAsync(id, ct);
        if (expense is null) return null;
        var money = Money.Parse(amount);
        if (categoryId != expense.CategoryId)
            await EnsureActiveCategoryAsync(categoryId, ct);
        expense.Change(money, categoryId, RequireDate(date), comment);
        await expenses.SaveChangesAsync(ct);
        return ToDto(expense);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var expense = await expenses.GetAsync(id, ct);
        if (expense is null) return false;
        expenses.Remove(expense);
        await expenses.SaveChangesAsync(ct);
        return true;
    }

    private async Task EnsureActiveCategoryAsync(Guid categoryId, CancellationToken ct)
    {
        var category = await categories.GetAsync(categoryId, ct)
            ?? throw new DomainException("Категорію не знайдено");
        if (category.IsArchived)
            throw new DomainException("Категорія в архіві, оберіть активну категорію");
    }

    private static DateOnly RequireDate(DateOnly? date) =>
        date ?? throw new DomainException("Вкажіть дату витрати");

    private static ExpenseDto ToDto(Expense e) => new(e.Id, e.Amount.ToString(), e.CategoryId, e.Date, e.Comment);
}
