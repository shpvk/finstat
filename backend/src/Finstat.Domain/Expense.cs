namespace Finstat.Domain;

public class Expense
{
    public const int MaxCommentLength = 200;

    private Expense() { }

    public Expense(Guid id, Money amount, Guid categoryId, DateOnly date, string? comment, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
        Change(amount, categoryId, date, comment);
    }

    public Guid Id { get; private set; }
    public Money Amount { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateOnly Date { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void Change(Money amount, Guid categoryId, DateOnly date, string? comment)
    {
        if (amount.Value <= 0)
            throw new DomainException("Сума має бути більшою за нуль");
        var normalized = NormalizeComment(comment);
        Amount = amount;
        CategoryId = categoryId;
        Date = date;
        Comment = normalized;
    }

    public static string? NormalizeComment(string? comment)
    {
        var trimmed = comment?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        if (trimmed.Length > MaxCommentLength)
            throw new DomainException($"Коментар не може бути довшим за {MaxCommentLength} символів");
        return trimmed;
    }
}
