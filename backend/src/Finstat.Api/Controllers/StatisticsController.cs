using Finstat.Api.Helpers;
using Finstat.Application.DTOs;
using Finstat.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finstat.Api.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController(StatisticsService service) : AppControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MonthStatisticsResponseDto>> Get([FromQuery] string? month, CancellationToken ct)
    {
        if (!MonthParser.TryParse(month, out var first))
            return Problem(MonthParser.InvalidMonthMessage, statusCode: StatusCodes.Status400BadRequest);
        var statistics = await service.GetAsync(first, ct);
        return Ok(statistics);
    }
}
