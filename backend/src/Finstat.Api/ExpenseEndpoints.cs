using System.Globalization;
using Finstat.Application;

namespace Finstat.Api;

public record ExpenseRequest(string? Amount, Guid CategoryId, DateOnly? Date, string? Comment);

public static class ExpenseEndpoints
{
    public static void MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/expenses").WithTags("Expenses");

        group.MapGet("/", async (string? month, ExpenseService service, CancellationToken ct) =>
            {
                if (!DateOnly.TryParseExact(month is null ? null : month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var first) || first is { Year: 9999, Month: 12 })
                    return Results.Problem("Вкажіть місяць у форматі РРРР-ММ", statusCode: StatusCodes.Status400BadRequest);
                return Results.Ok(await service.ListAsync(first, ct));
            })
            .WithName("ListExpenses")
            .Produces<List<ExpenseDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/", async (ExpenseRequest request, ExpenseService service, CancellationToken ct) =>
            {
                var expense = await service.CreateAsync(request.Amount, request.CategoryId, request.Date, request.Comment, ct);
                return TypedResults.Created($"/api/expenses/{expense.Id}", expense);
            })
            .WithName("CreateExpense");

        group.MapPut("/{id:guid}", async (Guid id, ExpenseRequest request, ExpenseService service, CancellationToken ct) =>
                await service.UpdateAsync(id, request.Amount, request.CategoryId, request.Date, request.Comment, ct) is { } expense
                    ? Results.Ok(expense)
                    : Results.NotFound())
            .WithName("UpdateExpense")
            .Produces<ExpenseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, ExpenseService service, CancellationToken ct) =>
                await service.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteExpense")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
