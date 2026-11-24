using Finstat.Domain.Entities;

namespace Finstat.Application.Repositories;

public interface ICategoryRepository
{
    Task<CategoryEntity?> GetAsync(Guid id, CancellationToken ct);
    Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken ct);
    Task<List<CategoryEntity>> ListAsync(bool includeArchived, CancellationToken ct);
    void Add(CategoryEntity category);
    Task SaveChangesAsync(CancellationToken ct);
}
