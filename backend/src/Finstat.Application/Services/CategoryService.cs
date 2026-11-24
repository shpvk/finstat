using Finstat.Application.DTOs;
using Finstat.Application.Repositories;
using Finstat.Domain.Entities;
using Finstat.Domain.Results;

namespace Finstat.Application.Services;

public class CategoryService(ICategoryRepository categories)
{
    public async Task<List<CategoryResponseDto>> ListAsync(bool includeArchived, CancellationToken ct)
    {
        var list = await categories.ListAsync(includeArchived, ct);
        var result = new List<CategoryResponseDto>();
        foreach (var category in list)
            result.Add(ToDto(category));
        return result;
    }

    public async Task<Result<CategoryResponseDto>> CreateAsync(string? name, CancellationToken ct)
    {
        var created = CategoryEntity.Create(Guid.NewGuid(), name);
        if (created.IsFailure)
            return Result<CategoryResponseDto>.Failure(created.Error);
        var category = created.Value;
        var unique = await EnsureUniqueAsync(category.Name, null, ct);
        if (unique.IsFailure)
            return Result<CategoryResponseDto>.Failure(unique.Error);
        categories.Add(category);
        await categories.SaveChangesAsync(ct);
        return Result<CategoryResponseDto>.Success(ToDto(category));
    }

    public async Task<Result<CategoryResponseDto>> RenameAsync(Guid id, string? name, CancellationToken ct)
    {
        var category = await categories.GetAsync(id, ct);
        if (category is null)
            return Result<CategoryResponseDto>.Failure(Error.NotFound());
        var renamed = category.Rename(name);
        if (renamed.IsFailure)
            return Result<CategoryResponseDto>.Failure(renamed.Error);
        var unique = await EnsureUniqueAsync(category.Name, id, ct);
        if (unique.IsFailure)
            return Result<CategoryResponseDto>.Failure(unique.Error);
        await categories.SaveChangesAsync(ct);
        return Result<CategoryResponseDto>.Success(ToDto(category));
    }

    public async Task<Result> ArchiveAsync(Guid id, CancellationToken ct)
    {
        var category = await categories.GetAsync(id, ct);
        if (category is null)
            return Result.Failure(Error.NotFound());
        category.Archive();
        await categories.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result> EnsureUniqueAsync(string name, Guid? excludeId, CancellationToken ct)
    {
        if (await categories.NameExistsAsync(name, excludeId, ct))
            return Result.Failure(Error.Validation("Категорія з такою назвою вже існує"));
        return Result.Success();
    }

    private static CategoryResponseDto ToDto(CategoryEntity category)
    {
        return new CategoryResponseDto(category.Id, category.Name, category.IsArchived);
    }
}
