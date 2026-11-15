using Finstat.Application;

namespace Finstat.Api;

public record CategoryRequest(string? Name);

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async (CategoryService service, CancellationToken ct, bool includeArchived = false) =>
                TypedResults.Ok(await service.ListAsync(includeArchived, ct)))
            .WithName("ListCategories");

        group.MapPost("/", async (CategoryRequest request, CategoryService service, CancellationToken ct) =>
            {
                var category = await service.CreateAsync(request.Name, ct);
                return TypedResults.Created($"/api/categories/{category.Id}", category);
            })
            .WithName("CreateCategory");

        group.MapPut("/{id:guid}", async (Guid id, CategoryRequest request, CategoryService service, CancellationToken ct) =>
                await service.RenameAsync(id, request.Name, ct) is { } category
                    ? Results.Ok(category)
                    : Results.NotFound())
            .WithName("RenameCategory")
            .Produces<CategoryDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/archive", async (Guid id, CategoryService service, CancellationToken ct) =>
                await service.ArchiveAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("ArchiveCategory")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
