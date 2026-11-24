using Finstat.Application.DTOs;
using Finstat.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finstat.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(CategoryService service) : AppControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryResponseDto>>> List([FromQuery] bool includeArchived, CancellationToken ct)
    {
        var categories = await service.ListAsync(includeArchived, ct);
        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CategoryRequestDto request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request.Name, ct);
        if (result.IsFailure)
            return Failure(result.Error);
        return Created($"/api/categories/{result.Value.Id}", result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryResponseDto>> Rename([FromRoute] Guid id, [FromBody] CategoryRequestDto request, CancellationToken ct)
    {
        var result = await service.RenameAsync(id, request.Name, ct);
        if (result.IsFailure)
            return Failure(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await service.ArchiveAsync(id, ct);
        if (result.IsFailure)
            return Failure(result.Error);
        return NoContent();
    }
}
