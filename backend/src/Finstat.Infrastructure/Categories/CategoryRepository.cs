using Finstat.Application;
using Finstat.Domain;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure.Categories;

public class CategoryRepository(FinstatDbContext db) : ICategoryRepository
{
    public Task<Category?> GetAsync(Guid id, CancellationToken ct) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken ct) =>
        db.Categories.AnyAsync(c => c.Name == name && c.Id != excludeId, ct);

    public Task<List<Category>> ListAsync(bool includeArchived, CancellationToken ct) =>
        db.Categories.Where(c => includeArchived || !c.IsArchived).OrderBy(c => c.Name).ToListAsync(ct);

    public void Add(Category category) => db.Categories.Add(category);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
