using Finstat.Domain;

namespace Finstat.Application;

public record CategoryDto(Guid Id, string Name, bool IsArchived);

public class CategoryService(ICategoryRepository categories)
{
    public async Task<List<CategoryDto>> ListAsync(bool includeArchived, CancellationToken ct) =>
        (await categories.ListAsync(includeArchived, ct)).Select(ToDto).ToList();

    public async Task<CategoryDto> CreateAsync(string? name, CancellationToken ct)
    {
        var category = new Category(Guid.NewGuid(), name!);
        await EnsureUniqueAsync(category.Name, null, ct);
        categories.Add(category);
        await categories.SaveChangesAsync(ct);
        return ToDto(category);
    }

    public async Task<CategoryDto?> RenameAsync(Guid id, string? name, CancellationToken ct)
    {
        var category = await categories.GetAsync(id, ct);
        if (category is null) return null;
        category.Rename(name!);
        await EnsureUniqueAsync(category.Name, id, ct);
        await categories.SaveChangesAsync(ct);
        return ToDto(category);
    }

    public async Task<bool> ArchiveAsync(Guid id, CancellationToken ct)
    {
        var category = await categories.GetAsync(id, ct);
        if (category is null) return false;
        category.Archive();
        await categories.SaveChangesAsync(ct);
        return true;
    }

    private async Task EnsureUniqueAsync(string name, Guid? excludeId, CancellationToken ct)
    {
        if (await categories.NameExistsAsync(name, excludeId, ct))
            throw new DomainException("Категорія з такою назвою вже існує");
    }

    private static CategoryDto ToDto(Category c) => new(c.Id, c.Name, c.IsArchived);
}
