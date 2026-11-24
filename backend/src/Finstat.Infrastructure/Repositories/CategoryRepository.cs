using Finstat.Application.Repositories;
using Finstat.Domain.Entities;
using Finstat.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure.Repositories;

public class CategoryRepository(FinstatDbContext db) : ICategoryRepository
{
    public async Task<CategoryEntity?> GetAsync(Guid id, CancellationToken ct)
    {
        var query = from category in db.Categories
                    where category.Id == id
                    select category;
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken ct)
    {
        var query = from category in db.Categories
                    where category.Name == name && category.Id != excludeId
                    select category;
        return await query.AnyAsync(ct);
    }

    public async Task<List<CategoryEntity>> ListAsync(bool includeArchived, CancellationToken ct)
    {
        var query = from category in db.Categories
                    where includeArchived || !category.IsArchived
                    orderby category.Name
                    select category;
        return await query.ToListAsync(ct);
    }

    public void Add(CategoryEntity category)
    {
        db.Categories.Add(category);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
