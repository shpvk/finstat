using Finstat.Application;

namespace Finstat.Api;

public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/statistics", async (string? month, StatisticsService service, CancellationToken ct) =>
                ExpenseEndpoints.TryParseMonth(month, out var first)
                    ? Results.Ok(await service.GetAsync(first, ct))
                    : ExpenseEndpoints.InvalidMonth())
            .WithTags("Statistics")
            .WithName("GetMonthStatistics")
            .Produces<MonthStatisticsDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
