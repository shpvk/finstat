using System.Globalization;
using Finstat.Application.DTOs;
using Finstat.Application.Repositories;
using Finstat.Domain.Entities;
using Finstat.Domain.Results;

namespace Finstat.Application.Services;

public class ExpenseService(IExpenseRepository expenses, ICategoryRepository categories)
{
    public async Task<List<ExpenseResponseDto>> ListAsync(DateOnly month, CancellationToken ct)
    {
        var from = new DateOnly(month.Year, month.Month, 1);
        var list = await expenses.ListAsync(from, from.AddMonths(1), ct);
        var result = new List<ExpenseResponseDto>();
        foreach (var expense in list)
            result.Add(ToDto(expense));
        return result;
    }

    public async Task<Result<ExpenseResponseDto>> CreateAsync(string? amount, Guid categoryId, DateOnly? date, string? comment, CancellationToken ct)
    {
        var money = ParseAmount(amount);
        if (money.IsFailure)
            return Result<ExpenseResponseDto>.Failure(money.Error);
        var category = await EnsureActiveCategoryAsync(categoryId, ct);
        if (category.IsFailure)
            return Result<ExpenseResponseDto>.Failure(category.Error);
        var requiredDate = RequireDate(date);
        if (requiredDate.IsFailure)
            return Result<ExpenseResponseDto>.Failure(requiredDate.Error);
        var created = ExpenseEntity.Create(Guid.NewGuid(), money.Value, categoryId, requiredDate.Value, comment, DateTime.UtcNow);
        if (created.IsFailure)
            return Result<ExpenseResponseDto>.Failure(created.Error);
        expenses.Add(created.Value);
        await expenses.SaveChangesAsync(ct);
        return Result<ExpenseResponseDto>.Success(ToDto(created.Value));
    }

    public async Task<Result<ExpenseResponseDto>> UpdateAsync(Guid id, string? amount, Guid categoryId, DateOnly? date, string? comment, CancellationToken ct)
    {
        var expense = await expenses.GetAsync(id, ct);
        if (expense is null)
            return Result<ExpenseResponseDto>.Failure(Error.NotFound());
        var money = ParseAmount(amount);
        if (money.IsFailure)
            return Result<ExpenseResponseDto>.Failure(money.Error);
        if (categoryId != expense.CategoryId)
        {
            var category = await EnsureActiveCategoryAsync(categoryId, ct);
            if (category.IsFailure)
                return Result<ExpenseResponseDto>.Failure(category.Error);
        }
        var requiredDate = RequireDate(date);
        if (requiredDate.IsFailure)
            return Result<ExpenseResponseDto>.Failure(requiredDate.Error);
        var changed = expense.Change(money.Value, categoryId, requiredDate.Value, comment);
        if (changed.IsFailure)
            return Result<ExpenseResponseDto>.Failure(changed.Error);
        await expenses.SaveChangesAsync(ct);
        return Result<ExpenseResponseDto>.Success(ToDto(expense));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        var expense = await expenses.GetAsync(id, ct);
        if (expense is null)
            return Result.Failure(Error.NotFound());
        expenses.Remove(expense);
        await expenses.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result> EnsureActiveCategoryAsync(Guid categoryId, CancellationToken ct)
    {
        var category = await categories.GetAsync(categoryId, ct);
        if (category is null)
            return Result.Failure(Error.Validation("Категорію не знайдено"));
        if (category.IsArchived)
            return Result.Failure(Error.Validation("Категорія в архіві, оберіть активну категорію"));
        return Result.Success();
    }

    private static Result<decimal> ParseAmount(string? amount)
    {
        if (!decimal.TryParse(amount?.Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
            return Result<decimal>.Failure(Error.Validation("Некоректна сума"));
        var valid = ExpenseEntity.ValidateAmount(value);
        if (valid.IsFailure)
            return Result<decimal>.Failure(valid.Error);
        return Result<decimal>.Success(value);
    }

    private static Result<DateOnly> RequireDate(DateOnly? date)
    {
        if (date is null)
            return Result<DateOnly>.Failure(Error.Validation("Вкажіть дату витрати"));
        return Result<DateOnly>.Success(date.Value);
    }

    internal static ExpenseResponseDto ToDto(ExpenseEntity expense)
    {
        return new ExpenseResponseDto(expense.Id, expense.Amount.ToString("0.00", CultureInfo.InvariantCulture), expense.CategoryId, expense.Date, expense.Comment);
    }
}
