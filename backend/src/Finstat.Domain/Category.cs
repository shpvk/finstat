namespace Finstat.Domain;

public class Category
{
    public const int MaxNameLength = 50;

    private Category() { }

    public Category(Guid id, string name)
    {
        Id = id;
        Name = Normalize(name);
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsArchived { get; private set; }

    public void Rename(string name) => Name = Normalize(name);

    public void Archive() => IsArchived = true;

    public static string Normalize(string? name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException("Назва категорії не може бути порожньою");
        if (trimmed.Length > MaxNameLength)
            throw new DomainException($"Назва категорії не може бути довшою за {MaxNameLength} символів");
        return trimmed;
    }
}
