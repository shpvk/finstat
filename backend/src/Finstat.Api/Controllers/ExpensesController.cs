using Finstat.Api.Helpers;
using Finstat.Application.DTOs;
using Finstat.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finstat.Api.Controllers;

[ApiController]
[Route("api/expenses")]
public class ExpensesController(ExpenseService service) : AppControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ExpenseResponseDto>>> List([FromQuery] string? month, CancellationToken ct)
    {
        if (!MonthParser.TryParse(month, out var first))
            return Problem(MonthParser.InvalidMonthMessage, statusCode: StatusCodes.Status400BadRequest);
        var expenses = await service.ListAsync(first, ct);
        return Ok(expenses);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponseDto>> Create([FromBody] ExpenseRequestDto request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request.Amount, request.CategoryId, request.Date, request.Comment, ct);
        if (result.IsFailure)
            return Failure(result.Error);
        return Created($"/api/expenses/{result.Value.Id}", result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseResponseDto>> Update([FromRoute] Guid id, [FromBody] ExpenseRequestDto request, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request.Amount, request.CategoryId, request.Date, request.Comment, ct);
        if (result.IsFailure)
            return Failure(result.Error);
        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        if (result.IsFailure)
            return Failure(result.Error);
        return NoContent();
    }
}
