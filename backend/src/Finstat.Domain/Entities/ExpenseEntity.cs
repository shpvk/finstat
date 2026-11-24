using System.Globalization;
using Finstat.Domain.Results;

namespace Finstat.Domain.Entities;

public class ExpenseEntity
{
    public const int MaxCommentLength = 200;
    public const decimal MaxAmount = 9999999999.99m;

    private ExpenseEntity() { }

    private ExpenseEntity(Guid id, decimal amount, Guid categoryId, DateOnly date, string? comment, DateTime createdAt)
    {
        Id = id;
        Amount = amount;
        CategoryId = categoryId;
        Date = date;
        Comment = comment;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateOnly Date { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<ExpenseEntity> Create(Guid id, decimal amount, Guid categoryId, DateOnly date, string? comment, DateTime createdAt)
    {
        var validated = Validate(amount, comment);
        if (validated.IsFailure)
            return Result<ExpenseEntity>.Failure(validated.Error);
        return Result<ExpenseEntity>.Success(new ExpenseEntity(id, amount, categoryId, date, validated.Value, createdAt));
    }

    public Result Change(decimal amount, Guid categoryId, DateOnly date, string? comment)
    {
        var validated = Validate(amount, comment);
        if (validated.IsFailure)
            return Result.Failure(validated.Error);
        Amount = amount;
        CategoryId = categoryId;
        Date = date;
        Comment = validated.Value;
        return Result.Success();
    }

    public static Result ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure(Error.Validation("Сума має бути більшою за нуль"));
        if (amount > MaxAmount)
            return Result.Failure(Error.Validation($"Сума не може перевищувати {MaxAmount.ToString("0.00", CultureInfo.InvariantCulture)}"));
        if (decimal.Round(amount, 2) != amount)
            return Result.Failure(Error.Validation("Сума може мати не більше двох знаків після коми"));
        return Result.Success();
    }

    private static Result<string?> Validate(decimal amount, string? comment)
    {
        var validAmount = ValidateAmount(amount);
        if (validAmount.IsFailure)
            return Result<string?>.Failure(validAmount.Error);
        var trimmed = comment?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return Result<string?>.Success(null);
        if (trimmed.Length > MaxCommentLength)
            return Result<string?>.Failure(Error.Validation($"Коментар не може бути довшим за {MaxCommentLength} символів"));
        return Result<string?>.Success(trimmed);
    }
}
