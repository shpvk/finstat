using Finstat.Domain;

namespace Finstat.Application;

public interface ICategoryRepository
{
    Task<Category?> GetAsync(Guid id, CancellationToken ct);
    Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken ct);
    Task<List<Category>> ListAsync(bool includeArchived, CancellationToken ct);
    void Add(Category category);
    Task SaveChangesAsync(CancellationToken ct);
}
