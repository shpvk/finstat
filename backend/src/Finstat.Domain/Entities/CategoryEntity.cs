using Finstat.Domain.Results;

namespace Finstat.Domain.Entities;

public class CategoryEntity
{
    public const int MaxNameLength = 50;

    private CategoryEntity() { }

    private CategoryEntity(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsArchived { get; private set; }

    public static Result<CategoryEntity> Create(Guid id, string? name)
    {
        var normalized = Normalize(name);
        if (normalized.IsFailure)
            return Result<CategoryEntity>.Failure(normalized.Error);
        return Result<CategoryEntity>.Success(new CategoryEntity(id, normalized.Value));
    }

    public Result Rename(string? name)
    {
        var normalized = Normalize(name);
        if (normalized.IsFailure)
            return Result.Failure(normalized.Error);
        Name = normalized.Value;
        return Result.Success();
    }

    public void Archive()
    {
        IsArchived = true;
    }

    private static Result<string> Normalize(string? name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return Result<string>.Failure(Error.Validation("Назва категорії не може бути порожньою"));
        if (trimmed.Length > MaxNameLength)
            return Result<string>.Failure(Error.Validation($"Назва категорії не може бути довшою за {MaxNameLength} символів"));
        return Result<string>.Success(trimmed);
    }
}
